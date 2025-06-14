namespace Shared.Dtos;

public class PostDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Category { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }

    public required string AuthorId { get; set; }
    public required UserDto Author { get; set; }

    public ICollection<byte[]>? Attachments { get; set; } = new List<byte[]>();
}