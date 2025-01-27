using AutoMapper;
using Domain.Entities;

namespace Application.Common.Payload.Dtos;

public class CurrencyDto
{
    public int R030 { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Currency, CurrencyDto>();
        }
    }
}