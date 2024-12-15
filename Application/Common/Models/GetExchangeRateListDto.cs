using System.Text.Json.Serialization;

namespace Application.Common.Models;

public class GetExchangeRateListDto
{
    [JsonInclude]
    public decimal MinSaleRateNb;
    [JsonInclude]
    public decimal MaxSaleRateNb;
    [JsonInclude]
    public decimal AverageSaleRateNb;
    [JsonInclude]
    public decimal MinPurchaseRateNb;
    [JsonInclude]
    public decimal MaxPurchaseRateNb;
    [JsonInclude]
    public decimal AveragePurchaseRateNb;
    [JsonInclude]
    public IEnumerable<ExchangeRateDto> ExchangeRates;
    
    public GetExchangeRateListDto(IEnumerable<ExchangeRateDto> exchangeRates)
    {
        ExchangeRates = exchangeRates;
        CalculateData();
    }
    
    private void CalculateData()
    {
        MinSaleRateNb = ExchangeRates.Min(x => x.SaleRateNb);
        MaxSaleRateNb = ExchangeRates.Max(x => x.SaleRateNb);
        AverageSaleRateNb = ExchangeRates.Average(x => x.SaleRateNb);
        MinPurchaseRateNb = ExchangeRates.Min(x => x.PurchaseRateNb);
        MaxPurchaseRateNb = ExchangeRates.Max(x => x.PurchaseRateNb);
        AveragePurchaseRateNb = ExchangeRates.Average(x => x.PurchaseRateNb);
    }
}