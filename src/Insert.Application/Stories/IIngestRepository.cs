using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IIngestRepository
{
    Task AddJobAsync(IngestJob job);
    Task<IngestJob?> GetJobByIdAsync(Guid id);
    Task<List<IngestJob>> GetAllJobsAsync();
    Task AddMediaAssetAsync(MediaAsset asset);
    Task SaveChangesAsync();

    Task<List<MediaAsset>> GetReadyUnattachedAssetsAsync();
    Task AddStoryMediaAsync(StoryMedia link);
    Task<List<StoryMedia>> GetMediaForStoryAsync(Guid storyId);
    Task<List<MediaAsset>> GetAssetsByIdsAsync(List<Guid> ids);

    Task<List<MediaAsset>> GetAllReadyAssetsAsync();
}