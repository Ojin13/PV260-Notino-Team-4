using System.ComponentModel.DataAnnotations;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Api.Validation;

/// <summary>Validates that every string in the collection is a well-formed email address.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EmailListAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<string> emails)
            return ValidationResult.Success;

        var invalid = emails
            .Where(e => !EmailValidator.IsValid(e))
            .ToList();

        return invalid.Count == 0
            ? ValidationResult.Success
            : new ValidationResult(
                $"Invalid email address(es): {string.Join(", ", invalid)}.",
                [validationContext.MemberName!]);
    }
}