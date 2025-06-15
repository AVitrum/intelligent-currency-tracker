using System.Text.Json.Serialization;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Post : BaseEntity
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PostCategory Category { get; set; } = PostCategory.General;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Language Language { get; set; } = Language.En;

    public required string UserId { get; set; }

    public ICollection<FileLink> Attachments { get; set; } = new List<FileLink>();
}