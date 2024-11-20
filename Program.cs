using PatientDataApp.Data;
using PatientDataApp.GraphQL;
using PatientDataApp.GraphQL.Types;
using PatientDataApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// Registrace služeb
builder.Services.AddScoped<IMriImageRepository, MriImageRepository>();
builder.Services.AddScoped<DicomService>();

// Konfigurace DbContext
builder.Services.AddDbContextFactory<PatientDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfigurace GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<UploadType>()
    .AddType<MriImageType>()
    .AddType<PatientType>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .AddAuthorization()
    .AddMaxFileSize(20_000_000) // 20MB limit
    .AddRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

app.UseCors();

// GraphQL endpoint a playground
app.MapGraphQL();
app.MapGraphQLVoyager("ui/voyager");

app.Run();
