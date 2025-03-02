using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Domain.Constants;
using Shared.Payload.Requests;

namespace Shared.Validation;

[AttributeUsage(AttributeTargets.Class)]
public class DateRangeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not ExchangeRateRequest dto)
        {
            return ValidationResult.Success;
        }

        if (string.IsNullOrEmpty(dto.StartDateString))
        {
            return new ValidationResult("Start date is required.", [nameof(dto.StartDateString)]);
        }

        if (!DateTime.TryParseExact(
                dto.StartDateString,
                DateConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var start))
        {
            return new ValidationResult("Invalid start date format.", [nameof(dto.StartDateString)]);
        }

        if (string.IsNullOrEmpty(dto.EndDateString))
        {
            dto.EndDateString = DateTime.UtcNow.ToString(DateConstants.DateFormat);
        }

        if (!DateTime.TryParseExact(
                dto.EndDateString,
                DateConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var end))
        {
            return new ValidationResult("Invalid end date format.", [nameof(dto.EndDateString)]);
        }

        if (start > end)
        {
            return new ValidationResult("Invalid date range. Start date must be before or equal to end date.",
                [nameof(dto.StartDateString), nameof(dto.EndDateString)]);
        }

        return ValidationResult.Success;
    }
}