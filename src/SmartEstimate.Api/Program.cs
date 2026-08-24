using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartEstimate.Api.Endpoints.Business;
using SmartEstimate.Api.Endpoints.Estimates;
using SmartEstimate.Api.Endpoints.Knowledge;
using SmartEstimate.Api.Endpoints.Pricing;
using SmartEstimate.Api.ExceptionHandling;
using SmartEstimate.Application;
using SmartEstimate.Documents;
using SmartEstimate.Infrastructure;
using SmartEstimate.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var supportedCultures = new[]
{
    new CultureInfo("uk-UA"),
    new CultureInfo("en-US"),
    new CultureInfo("de-DE")
};

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SmartEstimate.Api"));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(section => section.Value)
    .OfType<string>()
    .Concat((builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["culture"] = CultureInfo.CurrentUICulture.Name;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("uk-UA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders =
    [
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});
builder.Services.AddApplication();
builder.Services.AddDocuments();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartEstimate API",
        Version = "v1",
        Description = "SmartEstimate backend API foundation."
    });
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SmartEstimateDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((swaggerDocument, _) =>
        {
            swaggerDocument.Info.Title = GetSwaggerTitle(CultureInfo.CurrentUICulture);
            swaggerDocument.Info.Description = GetSwaggerDescription(CultureInfo.CurrentUICulture);
        });
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartEstimate API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapEstimateEndpoints();
app.MapEstimateDocumentEndpoints();
app.MapBusinessEndpoints();
app.MapKnowledgeEndpoints();
app.MapPricingEndpoints();

app.Run();

static string GetSwaggerTitle(CultureInfo culture) => culture.TwoLetterISOLanguageName switch
{
    "de" => "SmartEstimate API",
    "en" => "SmartEstimate API",
    _ => "SmartEstimate API"
};

static string GetSwaggerDescription(CultureInfo culture) => culture.TwoLetterISOLanguageName switch
{
    "de" => "Backend-API für SmartEstimate.",
    "en" => "SmartEstimate backend API.",
    _ => "Backend API для SmartEstimate."
};

public partial class Program;
