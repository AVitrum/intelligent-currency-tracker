using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Models;

public class ExchangeRateDto
{
    public required string Date { get; init; }
    public required Currency Currency { get; init; }
    public required decimal SaleRateNb { get; init; }
    public required decimal PurchaseRateNb { get; init; }
    public required decimal SaleRate { get; init; }
    public required decimal PurchaseRate { get; init; }
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ExchangeRate, ExchangeRateDto>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("dd.MM.yyyy")));
        }
    }
}