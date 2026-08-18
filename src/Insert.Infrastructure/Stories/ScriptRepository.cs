using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class ScriptRepository : IScriptRepository
{
    private readonly InsertDbContext _context;
    public ScriptRepository(InsertDbContext context) => _context = context;

    public Task<Script?> GetByStoryIdAsync(Guid storyId) =>
        _context.Scripts.FirstOrDefaultAsync(s => s.StoryId == storyId);

    public async Task AddScriptAsync(Script script) => await _context.Scripts.AddAsync(script);

    public Task<List<ScriptVersion>> GetVersionsAsync(Guid scriptId) =>
        _context.ScriptVersions.Where(v => v.ScriptId == scriptId).OrderByDescending(v => v.VersionNumber).ToListAsync();

    public Task<ScriptVersion?> GetLatestVersionAsync(Guid scriptId) =>
        _context.ScriptVersions.Where(v => v.ScriptId == scriptId).OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync();

    public async Task AddVersionAsync(ScriptVersion version) => await _context.ScriptVersions.AddAsync(version);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}