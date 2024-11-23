using AutoMapper;
using Domain.Entities;

namespace Application.Common.Models;

public class CreateBookModel
{
    public required string Title { get; init; }
    public required int Pages { get; init; }
    public required string Author { get; init; }
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateBookModel, Book>();
            CreateMap<Book, CreateBookModel>();
        }
    }
}

