using PatientDataApp.Data;
using PatientDataApp.GraphQL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Explicitní nastavení URL a portu pro .NET 8
builder.WebHost.UseUrls("http://localhost:8080");

// Přidání CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Přidání Antiforgery služby
builder.Services.AddAntiforgery();

// Přidání kontrolerů pro FHIR API
builder.Services.AddControllers();

// Přidání GraphQL serveru pro .NET 8
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

// Přidání DbContext
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Antiforgery middleware pro .NET 8
app.UseAntiforgery();

// Povolení CORS
app.UseCors();

// Přesměrování z root URL na /graphql
app.MapGet("/", () => Results.Redirect("/graphql"));

app.MapControllers();
app.MapGraphQL();

app.Run();
