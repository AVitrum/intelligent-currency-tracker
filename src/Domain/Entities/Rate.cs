using Domain.Common;

namespace Domain.Entities;

public class Rate : BaseEntity
{
    private readonly DateTime _date;
    public required Guid CurrencyId { get; init; }
    public Currency? Currency { get; init; }

    public decimal Value { get; init; }

    public DateTime Date
    {
        get => _date;
        init => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}