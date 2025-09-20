using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiPerfDashboard.Api.Models;

public sealed record MetricSummaryQuery : IValidatableObject
{
    private static readonly TimeSpan DefaultLookback = TimeSpan.FromHours(24);

    [Required]
    public DateTimeOffset From { get; init; } = DateTimeOffset.UtcNow.Subtract(DefaultLookback);

    [Required]
    public DateTimeOffset To { get; init; } = DateTimeOffset.UtcNow;

    [StringLength(128)]
    public string? ApiGroup { get; init; }
        = null;

    public static MetricSummaryQuery Create(DateTimeOffset? from, DateTimeOffset? to, string? apiGroup)
    {
        var now = DateTimeOffset.UtcNow;
        return new MetricSummaryQuery
        {
            From = (from ?? now.Subtract(DefaultLookback)).ToUniversalTime(),
            To = (to ?? now).ToUniversalTime(),
            ApiGroup = string.IsNullOrWhiteSpace(apiGroup) ? null : apiGroup
        };
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From > To)
        {
            yield return new ValidationResult(
                "The 'from' value must be earlier than the 'to' value.",
                new[] { nameof(From), nameof(To) });
        }
    }
}
