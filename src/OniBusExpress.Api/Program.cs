using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OniBusExpress.Api.Endpoints;
using OniBusExpress.Api.Http;
using OniBusExpress.Api.Observability;
using OniBusExpress.Api.Startup;
using OniBusExpress.Api.Validation;
using OniBusExpress.Application;
using OniBusExpress.Infrastructure;
using OniBusExpress.Infrastructure.Persistence;
using Serilog;

var aspNetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var isProductionLike = aspNetEnvironment is null
    || string.Equals(aspNetEnvironment, "Production", StringComparison.OrdinalIgnoreCase);

if (!isProductionLike)
{
    Env.Load(options: new LoadOptions(clobberExistingVars: false, onlyExactPath: false));
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? BuildConnectionStringFromParts(builder.Configuration)
    ?? throw new InvalidOperationException(
        "Banco de dados não configurado. Defina a connection string em ConnectionStrings__Default " +
        "ou as variáveis POSTGRES_USER, POSTGRES_PASSWORD e POSTGRES_DB (por exemplo, no arquivo .env).");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddValidatorsFromAssemblyContaining<CreateReservationRequestValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" });

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapRouteEndpoints();
app.MapTripEndpoints();
app.MapReservationEndpoints();

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

await app.MigrateAndSeedAsync();

app.Run();

static string? BuildConnectionStringFromParts(IConfiguration configuration)
{
    var database = configuration["POSTGRES_DB"];
    var username = configuration["POSTGRES_USER"];
    var password = configuration["POSTGRES_PASSWORD"];

    if (string.IsNullOrWhiteSpace(database)
        || string.IsNullOrWhiteSpace(username)
        || string.IsNullOrWhiteSpace(password))
    {
        return null;
    }

    var host = configuration["POSTGRES_HOST"] ?? "localhost";
    var port = configuration["POSTGRES_PORT"] ?? "5432";

    return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
}

public partial class Program;
