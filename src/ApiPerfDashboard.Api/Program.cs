using ApiPerfDashboard.Api.Configuration;
using ApiPerfDashboard.Api.Data;
using ApiPerfDashboard.Api.HealthChecks;
using ApiPerfDashboard.Api.Infrastructure;
using ApiPerfDashboard.Api.Models;
using ApiPerfDashboard.Api.Validation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .Configure(options =>
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("Postgres")
                ?? DatabaseOptions.DefaultConnectionString;
        }
    })
    .ValidateDataAnnotations()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ConnectionString),
        "A database connection string must be provided.")
    .Validate(
        options => options.CommandTimeoutSeconds > 0,
        "Database command timeout must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(options.ConnectionString)
    {
        DefaultCommandTimeout = options.CommandTimeoutSeconds
    };

    dataSourceBuilder.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
    return dataSourceBuilder.Build();
});

builder.Services.AddScoped<MetricRepository>();
builder.Services.AddHostedService<MetricSchemaInitializer>();
builder.Services.AddHealthChecks()
    .AddCheck<MetricsDbHealthCheck>("metrics-database", tags: new[] { "ready" });

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

var metricsGroup = app.MapGroup("/metrics")
    .WithTags("Metrics")
    .WithOpenApi();

metricsGroup.MapPost(string.Empty, async Task<IResult> (
        MetricRequest request,
        MetricRepository repository,
        CancellationToken cancellationToken) =>
    {
        var errors = ValidationExtensions.ValidateObject(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        await repository.InsertMetricAsync(request, cancellationToken);
        var response = new MetricIngestedResponse(request.Timestamp, request.Endpoint, request.ApiGroup);
        return Results.Created($"/metrics?endpoint={Uri.EscapeDataString(request.Endpoint)}", response);
    })
    .Produces<MetricIngestedResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem()
    .WithName("IngestMetric")
    .WithSummary("Ingest a single API performance datapoint.")
    .WithDescription("Accepts latency, status code and metadata for an API call.");

metricsGroup.MapGet("/summary", async Task<IResult> (
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? apiGroup,
        MetricRepository repository,
        CancellationToken cancellationToken) =>
    {
        var query = MetricSummaryQuery.Create(from, to, apiGroup);
        var errors = ValidationExtensions.ValidateObject(query);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var rows = await repository.GetSummaryAsync(query, cancellationToken);
        return Results.Ok(rows);
    })
    .Produces<IReadOnlyList<MetricSummaryRow>>(StatusCodes.Status200OK)
    .ProducesValidationProblem()
    .WithName("GetMetricSummary")
    .WithSummary("Retrieve aggregated performance metrics.")
    .WithDescription("Returns average latency, error percentage and volume by endpoint.");

app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.Run();
