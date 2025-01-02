using System.ComponentModel.DataAnnotations;
using Application.Common.Payload.Requests;

namespace Application.Common.Validation;

public class DateFormatAttribute : ValidationAttribute
{
    private const string DateFormat = "dd.MM.yyyy";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var dto = (ExchangeRateRequest)validationContext.ObjectInstance;
        if (!DateTime.TryParseExact(dto.StartDateString, DateFormat, null, System.Globalization.DateTimeStyles.None,
                out DateTime startDate) ||
            !DateTime.TryParseExact(dto.EndDateString, DateFormat, null, System.Globalization.DateTimeStyles.None,
                out DateTime endDate)) return new ValidationResult("Invalid date range or format");
        if (startDate <= endDate)
        {
            dto.Start = startDate;
            dto.End = endDate;
        }
        else
        {
            dto.Start = endDate;
            dto.End = startDate;
        }

        return ValidationResult.Success;
    }
}