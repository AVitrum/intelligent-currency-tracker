namespace Shared.Payload.Requests;

public class CreatePostRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Category { get; set; }
    public required string AuthorId { get; set; }
    public string Language { get; set; } = "En";
    public ICollection<(string, string)>? Attachments { get; set; }
}