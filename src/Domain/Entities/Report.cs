using System.Text.Json.Serialization;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Report : BaseEntity
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public bool IsResolved { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public required string SenderId { get; set; }

    public ICollection<FileLink> Attachments { get; set; } = new List<FileLink>();
}