using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class ScriptSummary
{
    public Script? Script { get; set; }
    public List<ScriptVersion> Versions { get; set; } = new();
}

public class ScriptService
{
    public const int WordsPerMinute = 150;

    private readonly IScriptRepository _repository;
    private readonly StoryService _storyService;

    public ScriptService(IScriptRepository repository, StoryService storyService)
    {
        _repository = repository;
        _storyService = storyService;
    }

    public static int CountWords(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return 0;

        var spokenLines = content
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            // metadata: SLUG:, DATE:, DURATION:
            .Where(line => !System.Text.RegularExpressions.Regex.IsMatch(line, @"^[A-Z][A-Z\s]*:"))
            // cue labels: ANCHOR INTRO, VO, SOT, PKG — short, all-caps, no sentence punctuation
            .Where(line => !System.Text.RegularExpressions.Regex.IsMatch(line, @"^[A-Z][A-Z0-9\s\-/]{0,40}$"));

        return string.Join(" ", spokenLines)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }

    public static int EstimateSeconds(int wordCount) =>
        (int)Math.Round(wordCount / (double)WordsPerMinute * 60);

    public async Task<ScriptSummary> GetForStoryAsync(Guid storyId)
    {
        var script = await _repository.GetByStoryIdAsync(storyId);
        if (script is null) return new ScriptSummary();

        return new ScriptSummary
        {
            Script = script,
            Versions = await _repository.GetVersionsAsync(script.Id)
        };
    }

    /// Autosave — overwrites the working draft in place. No version created.
    public async Task SaveDraftAsync(Guid storyId, string content, Guid userId)
    {
        var script = await _repository.GetByStoryIdAsync(storyId);

        if (script is null)
        {
            script = new Script
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                Content = content,
                UpdatedBy = userId,
            };
            await _repository.AddScriptAsync(script);
        }
        else
        {
            script.Content = content;
            script.UpdatedAt = DateTime.UtcNow;
            script.UpdatedBy = userId;
        }

        await _repository.SaveChangesAsync();
    }

    /// Deliberate checkpoint — snapshots the current draft into ScriptVersion.
    public async Task<ScriptVersion> SaveVersionAsync(Guid storyId, Guid authorId, string? changeNote = null)
    {
        var script = await _repository.GetByStoryIdAsync(storyId)
            ?? throw new InvalidOperationException("Belum ada draft untuk disimpan sebagai versi.");

        var existingCount = (await _repository.GetVersionsAsync(script.Id)).Count;
        var wordCount = CountWords(script.Content);

        var version = new ScriptVersion
        {
            Id = Guid.NewGuid(),
            ScriptId = script.Id,
            VersionNumber = existingCount + 1,
            AuthorId = authorId,
            Content = script.Content,
            WordCount = wordCount,
            EstimatedDurationSeconds = EstimateSeconds(wordCount),
            ChangeNote = changeNote,
        };

        await _repository.AddVersionAsync(version);
        await _repository.SaveChangesAsync();
        return version;
    }

    /// Save a version AND move the story to InReview.
    public async Task SubmitForReviewAsync(Guid storyId, Guid authorId, string? changeNote = null)
    {
        await SaveVersionAsync(storyId, authorId, changeNote);
        await _storyService.ChangeStatusAsync(storyId, StoryStatus.InReview, authorId);
    }
}