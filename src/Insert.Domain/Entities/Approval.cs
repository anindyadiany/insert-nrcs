namespace Insert.Domain.Entities;

public class Approval
{
    public Guid Id { get; set; }
    public Guid StoryId { get; set; }
    public Guid? ScriptVersionId { get; set; }
    public Guid ReviewerId { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ApprovalDecision
{
    Pending,
    Approved,
    Rejected,
    ChangesRequested
}