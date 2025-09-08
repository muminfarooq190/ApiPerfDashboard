using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string connStr = builder.Configuration.GetConnectionString("Postgres") ??
    "Host=localhost;Port=5432;Database=metrics;Username=metrics;Password=metrics";

app.MapPost("/metrics", async (Metric m) =>
{
    await using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();

    // Create table if not exists
    var createSql = @"
        CREATE TABLE IF NOT EXISTS api_metrics (
            id SERIAL PRIMARY KEY,
            timestamp TIMESTAMPTZ NOT NULL,
            endpoint TEXT NOT NULL,
            api_group TEXT,
            latency_ms INT NOT NULL,
            status_code INT NOT NULL
        );";
    await using (var createCmd = new NpgsqlCommand(createSql, conn))
    {
        await createCmd.ExecuteNonQueryAsync();
    }

    // Insert row
    var insertSql = "INSERT INTO api_metrics (timestamp, endpoint, api_group, latency_ms, status_code) VALUES (@ts, @ep, @grp, @lat, @sc)";
    await using (var cmd = new NpgsqlCommand(insertSql, conn))
    {
        cmd.Parameters.AddWithValue("@ts", m.Timestamp);
        cmd.Parameters.AddWithValue("@ep", m.Endpoint);
        cmd.Parameters.AddWithValue("@grp", m.ApiGroup ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@lat", m.LatencyMs);
        cmd.Parameters.AddWithValue("@sc", m.StatusCode);
        await cmd.ExecuteNonQueryAsync();
    }

    return Results.Ok(new { saved = true });
})
.WithName("PostMetric")
.WithOpenApi();

app.MapGet("/metrics/summary", async (DateTime from, DateTime to) =>
{
    await using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();

    var sql = @"
        SELECT endpoint,
               AVG(latency_ms) as avg_latency,
               100.0 * SUM(CASE WHEN status_code >= 400 THEN 1 ELSE 0 END) / COUNT(*) as error_pct,
               COUNT(*) as total
        FROM api_metrics
        WHERE timestamp BETWEEN @from AND @to
        GROUP BY endpoint
        ORDER BY endpoint";
    await using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@from", from);
    cmd.Parameters.AddWithValue("@to", to);

    var result = new List<object>();
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        result.Add(new {
            Endpoint = reader["endpoint"].ToString(),
            AvgLatency = reader["avg_latency"],
            ErrorPct = reader["error_pct"],
            Count = reader["total"]
        });
    }
    return Results.Ok(result);
})
.WithName("GetSummary")
.WithOpenApi();

app.Run();

record Metric(DateTime Timestamp, string Endpoint, int LatencyMs, int StatusCode, string? ApiGroup);
