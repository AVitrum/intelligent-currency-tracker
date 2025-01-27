using Domain.Common;

namespace Domain.Entities;

public class Currency : BaseEntity
{
    public required int R030 { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}