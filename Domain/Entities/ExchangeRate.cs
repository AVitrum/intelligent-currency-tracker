using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public DateTime Date
    {
        get => _date;
        init => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    
    private readonly DateTime _date;
    public required Currency Currency { get; init; }
    public decimal SaleRateNb { get; set; }
    public decimal PurchaseRateNb { get; set; }
    public decimal SaleRate { get; set; }
    public decimal PurchaseRate { get; set; }
}