using Domain.Common;

namespace Domain.Entities;

public class FileLink : BaseEntity
{
    public required string Key { get; set; }
    public required string Extension { get; set; }
}