using Domain.Common;

namespace Domain.Entities;

public class Book : BaseEntity
{
    public required string Title { get; set; }
    public required int Pages { get; set; }
    public required string Author { get; set; }
}