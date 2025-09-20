using ApiPerfDashboard.Api.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ApiPerfDashboard.Api.Infrastructure;

public sealed class MetricSchemaInitializer : IHostedService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly DatabaseOptions _options;
    private readonly ILogger<MetricSchemaInitializer> _logger;

    private const string CreateTableSql = @"
        CREATE TABLE IF NOT EXISTS api_metrics (
            id SERIAL PRIMARY KEY,
            timestamp TIMESTAMPTZ NOT NULL,
            endpoint TEXT NOT NULL,
            api_group TEXT NULL,
            latency_ms INT NOT NULL,
            status_code INT NOT NULL
        );";

    public MetricSchemaInitializer(
        NpgsqlDataSource dataSource,
        IOptions<DatabaseOptions> options,
        ILogger<MetricSchemaInitializer> logger)
    {
        _dataSource = dataSource;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring the api_metrics table exists.");
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = CreateTableSql;
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Database schema check completed successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
