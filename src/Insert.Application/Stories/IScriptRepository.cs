using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IScriptRepository
{
    Task<Script?> GetByStoryIdAsync(Guid storyId);
    Task AddScriptAsync(Script script);
    Task<List<ScriptVersion>> GetVersionsAsync(Guid scriptId);
    Task<ScriptVersion?> GetLatestVersionAsync(Guid scriptId);
    Task AddVersionAsync(ScriptVersion version);
    Task SaveChangesAsync();
}

