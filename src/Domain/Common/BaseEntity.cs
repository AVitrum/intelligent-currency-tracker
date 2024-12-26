namespace Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset LastModifiedAt { get; set; }
}