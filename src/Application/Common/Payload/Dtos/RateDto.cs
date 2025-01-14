using AutoMapper;
using Domain.Entities;

namespace Application.Common.Payload.Dtos;

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
                    opt.MapFrom(src => src.Date.ToString("dd.MM.yyyy")));
        }
    }
}