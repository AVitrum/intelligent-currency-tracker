using System.Text.Json.Serialization;
using AutoMapper;
using Domain.Constants;
using Domain.Entities;

namespace Shared.Dtos;

public class RateDto
{
    [JsonInclude]
    public decimal Value { get; set; }

    [JsonInclude]
    public decimal ValueCompareToPrevious { get; set; }

    [JsonInclude]
    public required string Date { get; set; }

    [JsonInclude]
    public int Year { get; set; }

    [JsonInclude]
    public int Month { get; set; }

    [JsonInclude]
    public int Day { get; set; }

    [JsonInclude]
    public int WeekNumber { get; set; }

    // public bool IsHoliday { get; init; }

    [JsonInclude]
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