namespace Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastModifiedAt { get; set; }
}