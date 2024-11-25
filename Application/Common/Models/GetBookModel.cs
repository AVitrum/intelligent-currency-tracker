using AutoMapper;
using Domain.Entities;

namespace Application.Common.Models;

public class GetBookModel
{
    public required string Title { get; init; }
    public required int Pages { get; init; }
    public required string Author { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Book, GetBookModel>();
        }
    }
}