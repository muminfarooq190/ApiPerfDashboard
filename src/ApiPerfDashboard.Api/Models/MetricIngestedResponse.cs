namespace ApiPerfDashboard.Api.Models;

public sealed record MetricIngestedResponse(
    DateTimeOffset Timestamp,
    string Endpoint,
    string? ApiGroup);
