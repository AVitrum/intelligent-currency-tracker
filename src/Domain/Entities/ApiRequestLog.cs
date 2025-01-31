using Domain.Common;

namespace Domain.Entities;

public class ApiRequestLog : BaseEntity
{
    public string? UserId { get; set; }
    public string? Referer { get; set; }
    public string Method { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string QueryString { get; set; } = null!;
    public string Body { get; set; } = null!;
}