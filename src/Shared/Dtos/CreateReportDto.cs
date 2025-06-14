namespace Shared.Dtos;

public class CreateReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;

    public ICollection<(string, string)>? Attachments { get; set; }
}