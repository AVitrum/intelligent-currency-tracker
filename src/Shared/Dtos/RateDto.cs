using AutoMapper;
using Domain.Constants;
using Domain.Entities;

namespace Shared.Dtos;

public class RateDto
{
    public decimal Value { get; init; }
    public decimal ValueCompareToPrevious { get; init; }
    public required string Date { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public int Day { get; init; }
    public int WeekNumber { get; init; }

    // public bool IsHoliday { get; init; }

    public required CurrencyDto Currency { get; init; }

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