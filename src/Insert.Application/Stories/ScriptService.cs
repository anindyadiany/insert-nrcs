using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class ScriptSummary
{
    public Script? Script { get; set; }
    public ScriptVersion? LatestVersion { get; set; }
}

public class ScriptService
{
    private const int WordsPerMinute = 150;

    private readonly IScriptRepository _repository;
    private readonly StoryService _storyService;

    public ScriptService(IScriptRepository repository, StoryService storyService)
    {
        _repository = repository;
        _storyService = storyService;
    }

    public async Task<ScriptSummary> GetForStoryAsync(Guid storyId)
    {
        var script = await _repository.GetByStoryIdAsync(storyId);
        if (script is null) return new ScriptSummary();

        var latest = await _repository.GetLatestVersionAsync(script.Id);
        return new ScriptSummary { Script = script, LatestVersion = latest };
    }

    public async Task<ScriptVersion> SubmitScriptAsync(Guid storyId, string content, Guid authorId)
    {
        var script = await _repository.GetByStoryIdAsync(storyId);
        if (script is null)
        {
            script = new Script { Id = Guid.NewGuid(), StoryId = storyId };
            await _repository.AddScriptAsync(script);
        }

        var existingCount = (await _repository.GetVersionsAsync(script.Id)).Count;

        var wordCount = string.IsNullOrWhiteSpace(content)
            ? 0
            : content.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        var estimatedSeconds = (int)Math.Round(wordCount / (double)WordsPerMinute * 60);

        var version = new ScriptVersion
        {
            Id = Guid.NewGuid(),
            ScriptId = script.Id,
            VersionNumber = existingCount + 1,
            AuthorId = authorId,
            Content = content,
            WordCount = wordCount,
            EstimatedDurationSeconds = estimatedSeconds,
        };

        await _repository.AddVersionAsync(version);
        await _repository.SaveChangesAsync();

        await _storyService.ChangeStatusAsync(storyId, StoryStatus.InReview, authorId);

        return version;
    }
}