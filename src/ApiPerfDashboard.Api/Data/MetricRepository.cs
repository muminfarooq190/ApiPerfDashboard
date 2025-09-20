using ApiPerfDashboard.Api.Configuration;
using ApiPerfDashboard.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ApiPerfDashboard.Api.Data;

public sealed class MetricRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly DatabaseOptions _options;
    private readonly ILogger<MetricRepository> _logger;

    public MetricRepository(
        NpgsqlDataSource dataSource,
        IOptions<DatabaseOptions> options,
        ILogger<MetricRepository> logger)
    {
        _dataSource = dataSource;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InsertMetricAsync(MetricRequest metric, CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO api_metrics (timestamp, endpoint, api_group, latency_ms, status_code)
            VALUES (@timestamp, @endpoint, @api_group, @latency_ms, @status_code);";
        command.CommandTimeout = _options.CommandTimeoutSeconds;

        command.Parameters.AddWithValue("@timestamp", metric.Timestamp.UtcDateTime);
        command.Parameters.AddWithValue("@endpoint", metric.Endpoint);
        command.Parameters.AddWithValue("@api_group", metric.ApiGroup ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@latency_ms", metric.LatencyMs);
        command.Parameters.AddWithValue("@status_code", metric.StatusCode);

        _logger.LogDebug(
            "Writing metric for {Endpoint} (status: {StatusCode}, latency: {Latency}ms)",
            metric.Endpoint,
            metric.StatusCode,
            metric.LatencyMs);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MetricSummaryRow>> GetSummaryAsync(
        MetricSummaryQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var sql = new System.Text.StringBuilder(@"
            SELECT endpoint,
                   api_group,
                   AVG(latency_ms) AS avg_latency,
                   100.0 * SUM(CASE WHEN status_code >= 400 THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS error_pct,
                   COUNT(*) AS total
            FROM api_metrics
            WHERE timestamp BETWEEN @from AND @to");

        command.Parameters.AddWithValue("@from", query.From.UtcDateTime);
        command.Parameters.AddWithValue("@to", query.To.UtcDateTime);

        if (!string.IsNullOrWhiteSpace(query.ApiGroup))
        {
            sql.Append(" AND api_group = @api_group");
            command.Parameters.AddWithValue("@api_group", query.ApiGroup);
        }

        sql.Append(" GROUP BY endpoint, api_group ORDER BY endpoint, api_group");
        command.CommandText = sql.ToString();
        command.CommandTimeout = _options.CommandTimeoutSeconds;

        var rows = new List<MetricSummaryRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var endpoint = reader.GetString(0);
            var apiGroup = await reader.IsDBNullAsync(1, cancellationToken)
                ? null
                : reader.GetString(1);
            var avgLatency = reader.IsDBNull(2) ? 0d : reader.GetDouble(2);
            var errorPct = reader.IsDBNull(3) ? 0d : reader.GetDouble(3);
            var total = reader.GetInt64(4);

            rows.Add(new MetricSummaryRow(
                endpoint,
                apiGroup,
                Math.Round(avgLatency, 2, MidpointRounding.AwayFromZero),
                Math.Round(errorPct, 2, MidpointRounding.AwayFromZero),
                total));
        }

        return rows;
    }
}
