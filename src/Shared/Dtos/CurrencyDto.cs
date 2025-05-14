using System.Text.Json.Serialization;
using AutoMapper;
using Domain.Entities;

namespace Shared.Dtos;

public class CurrencyDto
{
    [JsonInclude]
    public int R030 { get; set; }

    [JsonInclude]
    public string Code { get; set; } = string.Empty;

    [JsonInclude]
    public string Name { get; set; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Currency, CurrencyDto>();
        }
    }
}