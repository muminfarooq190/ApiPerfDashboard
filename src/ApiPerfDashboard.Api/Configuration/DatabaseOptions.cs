using System.ComponentModel.DataAnnotations;

namespace ApiPerfDashboard.Api.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=metrics;Username=metrics;Password=metrics";

    /// <summary>
    /// Connection string used to connect to the metrics database.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = DefaultConnectionString;

    /// <summary>
    /// Default timeout applied to SQL commands executed by the API.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int CommandTimeoutSeconds { get; set; } = 30;
}
