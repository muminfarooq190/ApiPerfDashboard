namespace ApiPerfDashboard.Api.Models;

public sealed record MetricSummaryRow(
    string Endpoint,
    string? ApiGroup,
    double AverageLatencyMs,
    double ErrorPercentage,
    long TotalRequests);
