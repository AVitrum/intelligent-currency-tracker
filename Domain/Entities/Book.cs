using Domain.Common;

namespace Domain.Entities;

public class Book : BaseEntity
{
    public required string Title { get; set; }
}