using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class UpdateStoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Program { get; set; }
    public StoryPriority Priority { get; set; } = StoryPriority.Normal;
    public DateTime? Deadline { get; set; }
    public string? Notes { get; set; }
}

public class StoryService
{
    private readonly IStoryRepository _repository;
    private readonly StoryWorkflowService _workflow;
    private readonly AuditLogService _auditLog;

    public StoryService(IStoryRepository repository, StoryWorkflowService workflow, AuditLogService auditLog)
    {
        _repository = repository;
        _workflow = workflow;
        _auditLog = auditLog;
    }

    public async Task<Story> CreateStoryAsync(CreateStoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        var story = new Story
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = GenerateSlug(request.Title),
            Category = request.Category,
            Program = request.Program,
            Priority = request.Priority,
            Status = StoryStatus.Draft,
            Deadline = request.Deadline,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(story);
        await _repository.SaveChangesAsync();
        return story;
    }

    public Task<List<Story>> GetAllStoriesAsync() => _repository.GetAllAsync();
    public Task<Story?> GetStoryByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task ChangeStatusAsync(Guid storyId, StoryStatus newStatus, Guid userId)
    {
        var story = await _repository.GetByIdAsync(storyId)
            ?? throw new KeyNotFoundException("Story not found.");

        var oldStatus = story.Status;
        _workflow.Transition(story, newStatus);
        await _repository.SaveChangesAsync();

        await _auditLog.LogAsync(userId, "StatusChanged", "Story", storyId, oldStatus.ToString(), newStatus.ToString());
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        return slug + "-" + Guid.NewGuid().ToString("N")[..6];
    }

    public async Task UpdateStoryAsync(Guid id, UpdateStoryRequest request)
    {
        var story = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Story not found.");

        story.Title = request.Title.Trim();
        story.Category = request.Category;
        story.Program = request.Program;
        story.Priority = request.Priority;
        story.Deadline = request.Deadline;
        story.Notes = request.Notes;
        story.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();
    }
}