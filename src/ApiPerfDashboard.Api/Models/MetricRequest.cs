using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiPerfDashboard.Api.Models;

public sealed record MetricRequest : IValidatableObject
{
    [Required]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    [Required]
    [StringLength(256)]
    public string Endpoint { get; init; } = string.Empty;

    [StringLength(128)]
    public string? ApiGroup { get; init; }
        = null;

    [Range(0, int.MaxValue)]
    public int LatencyMs { get; init; }
        = 0;

    [Range(100, 599)]
    public int StatusCode { get; init; }
        = 200;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Timestamp > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            yield return new ValidationResult(
                "The metric timestamp cannot be more than five minutes in the future.",
                new[] { nameof(Timestamp) });
        }

        if (!string.IsNullOrWhiteSpace(Endpoint) && !Endpoint.StartsWith('/'))
        {
            yield return new ValidationResult(
                "Endpoint values should be relative paths and start with '/'.",
                new[] { nameof(Endpoint) });
        }
    }
}
