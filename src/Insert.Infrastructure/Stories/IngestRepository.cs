using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class IngestRepository : IIngestRepository
{
    private readonly InsertDbContext _context;
    public IngestRepository(InsertDbContext context) => _context = context;
    public Task<List<MediaAsset>> GetAssetsByIdsAsync(List<Guid> ids) =>
    _context.MediaAssets.Where(a => ids.Contains(a.Id)).ToListAsync();
    public async Task AddJobAsync(IngestJob job) => await _context.IngestJobs.AddAsync(job);

    public Task<IngestJob?> GetJobByIdAsync(Guid id) =>
        _context.IngestJobs.FirstOrDefaultAsync(j => j.Id == id);

    public Task<List<IngestJob>> GetAllJobsAsync() =>
        _context.IngestJobs.OrderByDescending(j => j.CreatedAt).ToListAsync();

    public async Task AddMediaAssetAsync(MediaAsset asset) => await _context.MediaAssets.AddAsync(asset);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task<List<MediaAsset>> GetReadyUnattachedAssetsAsync()
    {
        var attachedIds = await _context.StoryMedias.Select(sm => sm.MediaAssetId).ToListAsync();
        return await _context.MediaAssets
            .Where(a => a.IngestStatus == MediaIngestStatus.Ready && !attachedIds.Contains(a.Id))
            .ToListAsync();
    }

    public async Task AddStoryMediaAsync(StoryMedia link) => await _context.StoryMedias.AddAsync(link);

    public Task<List<StoryMedia>> GetMediaForStoryAsync(Guid storyId) =>
        _context.StoryMedias.Where(sm => sm.StoryId == storyId).ToListAsync();

    public Task<List<MediaAsset>> GetAllReadyAssetsAsync() =>
    _context.MediaAssets.Where(a => a.IngestStatus == MediaIngestStatus.Ready).ToListAsync();
}