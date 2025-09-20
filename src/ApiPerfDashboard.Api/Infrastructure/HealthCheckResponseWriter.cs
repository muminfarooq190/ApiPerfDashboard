using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiPerfDashboard.Api.Infrastructure;

public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            results = report.Entries.Select(pair => new
            {
                name = pair.Key,
                status = pair.Value.Status.ToString(),
                description = pair.Value.Description,
                duration = pair.Value.Duration,
                error = pair.Value.Exception?.Message
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
