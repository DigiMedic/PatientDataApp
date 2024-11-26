using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.GraphQL;
using PatientDataApp.GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using PatientDataApp.Utils;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Načtení .env souboru pouze v development prostředí
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

// Konfigurace loggingu
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Vytvoření logger factory pro databázové logování
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var dbLogger = loggerFactory.CreateLogger("Database");

// Configure connection string
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                      $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                      $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
                      $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
var configData = new Dictionary<string, string?>
{
    {"ConnectionStrings:DefaultConnection", connectionString}
};
builder.Configuration.AddInMemoryCollection(configData);

// Přidání health checks s podporou pro PostgreSQL
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PatientDbContext>("Database")
    .AddNpgSql(
        connectionString,
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" },
        timeout: TimeSpan.FromSeconds(30))
    .AddCheck("API", () => HealthCheckResult.Healthy());

// Add DB Context
builder.Services.AddDbContext<PatientDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
    })
    .UseSnakeCaseNamingConvention();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add TestDataGenerator as a scoped service
builder.Services.AddScoped<TestDataGenerator>();

// Konfigurace JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new DateTimeJsonConverter());
});

// Konfigurace CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (bool.Parse(Environment.GetEnvironmentVariable("CORS_ALLOW_CREDENTIALS") ?? "false"))
        {
            // Pokud povolujeme credentials, musíme specifikovat konkrétní origins
            policy.WithOrigins("http://localhost:3000", "http://localhost:8080")
                 .AllowCredentials();
        }
        else
        {
            // Pokud nepovolujeme credentials, můžeme použít AllowAnyOrigin
            policy.AllowAnyOrigin();
        }
        
        if (bool.Parse(Environment.GetEnvironmentVariable("CORS_ALLOW_ANY_METHOD") ?? "false"))
        {
            policy.AllowAnyMethod();
        }
        
        if (bool.Parse(Environment.GetEnvironmentVariable("CORS_ALLOW_ANY_HEADER") ?? "false"))
        {
            policy.AllowAnyHeader();
        }
    });
});

// Konfigurace JWT autentizace
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT key is not configured"))
            )
        };
    });

// Konfigurace GraphQL serveru
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<PatientType>()
    .AddType<DiagnosticResultType>()
    .AddType<MriImageType>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .RegisterService<IDbContextFactory<PatientDbContext>>(ServiceKind.Synchronized)
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
        opt.ExecutionTimeout = TimeSpan.FromMinutes(int.Parse(Environment.GetEnvironmentVariable("GRAPHQL_EXECUTION_TIMEOUT_MINUTES") ?? "5"));
    })
    .ModifyOptions(opt =>
    {
        opt.UseXmlDocumentation = true;
        opt.SortFieldsByName = true;
    })
    .InitializeOnStartup();

var app = builder.Build();

// Apply migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
        dbContext.Database.EnsureDeleted(); // Reset database
        dbContext.Database.Migrate();
    }
    app.Logger.LogInformation("Database migrated successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while migrating the database");
    throw;
}

// Vývojové prostředí
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.Logger.LogInformation("Running in Development mode");
}

app.Logger.LogInformation("Configuring middleware pipeline...");

// CORS
app.UseCors();

// Přidání endpointů
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.Logger.LogInformation("Configuring GraphQL endpoint...");
app.MapGraphQL();

app.Logger.LogInformation("Application configured and ready to start");

// Spuštění aplikace
await app.RunAsync();
