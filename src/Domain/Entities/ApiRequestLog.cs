using System.Text.Json.Serialization;
using Domain.Common;

namespace Domain.Entities;

public class ApiRequestLog : BaseEntity
{
    [JsonIgnore] public string? UserId { get; set; }

    [JsonIgnore] public string? Referer { get; set; }

    [JsonIgnore] public string Method { get; set; } = null!;

    [JsonIgnore] public string Path { get; set; } = null!;

    [JsonIgnore] public string QueryString { get; set; } = null!;

    [JsonIgnore] public string Body { get; set; } = null!;
}