using Domain.Common;

namespace Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public DateTime Date
    {
        get => _date;
        init => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    
    private readonly DateTime _date;
    public required string? Currency { get; init; }
    public decimal SaleRateNb { get; init; }
    public decimal PurchaseRateNb { get; init; }
    public decimal SaleRate { get; init; }
    public decimal PurchaseRate { get; init; }
}