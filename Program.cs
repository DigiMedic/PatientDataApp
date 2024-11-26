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

// Získání connection stringu
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback na environment proměnné
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "patientdb";
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};Command Timeout=30;Internal Command Timeout=30";
}

// Přidání health checks s podporou pro PostgreSQL
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PatientDbContext>("Database")
    .AddNpgSql(
        connectionString,
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" },
        timeout: TimeSpan.FromSeconds(30))
    .AddCheck("API", () => HealthCheckResult.Healthy());

// Konfigurace DbContext
builder.Services.AddDbContextFactory<PatientDbContext>(options =>
{
    dbLogger.LogInformation("Attempting to connect to database...");

    // Logování connection stringu bez citlivých údajů
    var sanitizedConnectionString = connectionString.Replace(Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres", "********");
    dbLogger.LogInformation($"Using database connection string: {sanitizedConnectionString}");

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

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

// Automatická migrace databáze při startu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PatientDbContext>();
        dbLogger.LogInformation("Attempting to migrate database...");
        context.Database.Migrate();
        dbLogger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
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
