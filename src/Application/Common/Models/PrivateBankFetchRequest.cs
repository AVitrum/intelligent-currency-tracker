namespace Application.Common.Models;

public class FetchRequest
{
    public required List<string> Dates { get; set; }
    public required string UrlTemplate { get; set; }
}