using ApiPerfDashboard.Api.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ApiPerfDashboard.Api.HealthChecks;

public sealed class MetricsDbHealthCheck : IHealthCheck
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly DatabaseOptions _options;
    private readonly ILogger<MetricsDbHealthCheck> _logger;

    public MetricsDbHealthCheck(
        NpgsqlDataSource dataSource,
        IOptions<DatabaseOptions> options,
        ILogger<MetricsDbHealthCheck> logger)
    {
        _dataSource = dataSource;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = _options.CommandTimeoutSeconds;
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify connectivity to the metrics database.");
            return HealthCheckResult.Unhealthy(
                "Unable to connect to the metrics database.",
                ex);
        }
    }
}
