using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class StoryWorkflowService
{
    private static readonly Dictionary<StoryStatus, StoryStatus[]> AllowedTransitions = new()
    {
        [StoryStatus.Draft] = new[] { StoryStatus.Assigned, StoryStatus.Killed },
        [StoryStatus.Assigned] = new[] { StoryStatus.InProgress, StoryStatus.Killed },
        [StoryStatus.InProgress] = new[] { StoryStatus.InReview, StoryStatus.Killed },
        [StoryStatus.InReview] = new[] { StoryStatus.Approved, StoryStatus.InProgress, StoryStatus.Killed },
        [StoryStatus.Approved] = new[] { StoryStatus.Published, StoryStatus.Killed },
        [StoryStatus.Published] = Array.Empty<StoryStatus>(),
        [StoryStatus.Killed] = Array.Empty<StoryStatus>(),
    };

    public bool CanTransition(StoryStatus from, StoryStatus to) =>
        AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public void Transition(Story story, StoryStatus to)
    {
        if (!CanTransition(story.Status, to))
            throw new InvalidOperationException($"Cannot transition story from {story.Status} to {to}.");

        story.Status = to;
        story.UpdatedAt = DateTime.UtcNow;
    }
}