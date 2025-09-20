using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ApiPerfDashboard.Api.Validation;

public static class ValidationExtensions
{
    public static Dictionary<string, string[]> ValidateObject<T>(T instance)
        where T : class
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty),
                (result, memberName) => new { memberName, result.ErrorMessage })
            .Where(item => !string.IsNullOrWhiteSpace(item.ErrorMessage))
            .GroupBy(item => item.memberName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.ErrorMessage!)
                    .Distinct()
                    .ToArray());
    }
}
