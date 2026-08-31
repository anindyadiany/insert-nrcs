namespace Insert.Domain.Entities;

public class Rundown
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Program { get; set; }
    public DateTime AirDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}