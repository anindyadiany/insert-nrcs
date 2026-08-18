namespace Insert.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid StoryId { get; set; }
    public Guid ReporterId { get; set; }
    public string? Location { get; set; }
    public string? Event { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Brief { get; set; }
    public string? Notes { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
}

public enum AssignmentStatus
{
    Pending,
    Accepted,
    Declined,
    Cancelled
}