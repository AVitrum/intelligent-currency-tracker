namespace Shared.Dtos;

public class ReportDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public required string SenderId { get; set; }
    public bool IsResolved { get; set; }
    public ICollection<byte[]>? Attachments { get; set; }
}