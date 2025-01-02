namespace Application.Common.Payload.Requests;

public class FetchRequest
{
    public required List<string> Dates { get; set; }
    public required string UrlTemplate { get; set; }
}