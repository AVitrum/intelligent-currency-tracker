using AutoMapper;
using Domain.Constans;
using Domain.Entities;

namespace Shared.Dtos;

public class RateDto
{
    public decimal Value { get; init; }
    public required string Date { get; init; }

    public required CurrencyDto Currency { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Rate, RateDto>()
                .ForMember(dest => dest.Currency, opt =>
                    opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.Date, opt =>
                    opt.MapFrom(src => src.Date.ToString(DateConstants.DateFormat)));
        }
    }
}