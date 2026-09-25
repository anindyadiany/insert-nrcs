using System.Security.Cryptography;
using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class IngestService
{
    private readonly IIngestRepository _repository;
    private readonly IMediaProcessor _mediaProcessor;
    private static readonly string StorageRoot = "/Users/anindya/work/transtv/nrcs/IngestStorage";

    public IngestService(IIngestRepository repository, IMediaProcessor mediaProcessor)
    {
        _repository = repository;
        _mediaProcessor = mediaProcessor;
    }

    public async Task<IngestJob> CreateJobAsync(Guid userId, string sourceFilePath, string originalFilename, string sourceType = "upload")
    {
        var job = new IngestJob
        {
            Id = Guid.NewGuid(),
            SourceType = sourceType,
            SourceFilePath = sourceFilePath,
            OriginalFilename = originalFilename,
            Status = IngestJobStatus.Queued,
            Progress = 0,
            CreatedBy = userId,
        };

        await _repository.AddJobAsync(job);
        await _repository.SaveChangesAsync();
        return job;
    }

    public Task<List<IngestJob>> GetQueueAsync() => _repository.GetAllJobsAsync();

    public async Task RetryJobAsync(Guid jobId)
    {
        var job = await _repository.GetJobByIdAsync(jobId)
            ?? throw new KeyNotFoundException("Ingest job not found.");

        job.Status = IngestJobStatus.Queued;
        job.Progress = 0;
        job.ErrorMessage = null;
        await _repository.SaveChangesAsync();
    }

    /// Pause a job that hasn't started processing yet, so the worker skips it in the queue.
    public async Task PauseJobAsync(Guid jobId)
    {
        var job = await _repository.GetJobByIdAsync(jobId)
            ?? throw new KeyNotFoundException("Ingest job not found.");

        if (job.Status != IngestJobStatus.Queued)
            throw new InvalidOperationException("Hanya job yang masih Queued yang bisa di-pause.");

        job.Status = IngestJobStatus.Paused;
        await _repository.SaveChangesAsync();
    }

    /// Resume a paused job, putting it back in the queue.
    public async Task ResumeJobAsync(Guid jobId)
    {
        var job = await _repository.GetJobByIdAsync(jobId)
            ?? throw new KeyNotFoundException("Ingest job not found.");

        if (job.Status != IngestJobStatus.Paused)
            throw new InvalidOperationException("Hanya job yang sedang Paused yang bisa di-resume.");

        job.Status = IngestJobStatus.Queued;
        await _repository.SaveChangesAsync();
    }

    /// Cancel a job that's still Queued/Paused, or ask an in-flight Processing job to stop
    /// at its next checkpoint (see ProcessJobAsync).
    public async Task CancelJobAsync(Guid jobId)
    {
        var job = await _repository.GetJobByIdAsync(jobId)
            ?? throw new KeyNotFoundException("Ingest job not found.");

        if (job.Status is IngestJobStatus.Completed or IngestJobStatus.Cancelled)
            throw new InvalidOperationException("Job ini sudah selesai atau sudah dibatalkan.");

        job.Status = IngestJobStatus.Cancelled;
        job.ErrorMessage = "Dibatalkan oleh user.";
        await _repository.SaveChangesAsync();
    }

    public async Task ProcessJobAsync(Guid jobId)
    {
        var job = await _repository.GetJobByIdAsync(jobId)
            ?? throw new KeyNotFoundException("Ingest job not found.");

        // Skip jobs that were paused/cancelled between being picked up and now.
        if (job.Status != IngestJobStatus.Queued) return;

        try
        {
            job.Status = IngestJobStatus.Processing;
            job.Progress = 10;
            await _repository.SaveChangesAsync();

            var storageDir = Path.Combine(StorageRoot, "Ready");
            Directory.CreateDirectory(storageDir);

            var storedFilename = $"{job.Id}{Path.GetExtension(job.OriginalFilename)}";
            var destinationPath = Path.Combine(storageDir, storedFilename);

            using (var sourceStream = File.OpenRead(job.SourceFilePath))
            using (var destStream = File.Create(destinationPath))
            {
                await sourceStream.CopyToAsync(destStream);
            }
            job.Progress = 60;
            await _repository.SaveChangesAsync();

            if (await IsCancelledAsync(jobId)) { CleanupPartialFile(destinationPath); return; }

            string checksum;
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(destinationPath))
            {
                var hash = await sha256.ComputeHashAsync(stream);
                checksum = Convert.ToHexString(hash);
            }
            job.Progress = 90;
            await _repository.SaveChangesAsync();

            if (await IsCancelledAsync(jobId)) { CleanupPartialFile(destinationPath); return; }

            var probe = await _mediaProcessor.ProbeAsync(destinationPath);
            var thumbnailPath = await _mediaProcessor.GenerateThumbnailAsync(destinationPath, Path.Combine(StorageRoot, "Thumbnails"));
            var proxyPath = await _mediaProcessor.GenerateProxyAsync(destinationPath, Path.Combine(StorageRoot, "Proxies"));

            var asset = new MediaAsset
            {
                Id = Guid.NewGuid(),
                IngestJobId = job.Id,
                Filename = storedFilename,
                OriginalFilename = job.OriginalFilename,
                StoragePath = destinationPath,
                FileSize = new FileInfo(destinationPath).Length,
                Checksum = checksum,
                DurationSeconds = probe.DurationSeconds,
                Format = probe.Format,
                Codec = probe.Codec,
                Resolution = probe.Resolution,
                FrameRate = probe.FrameRate,
                ThumbnailPath = thumbnailPath,
                ProxyPath = proxyPath,
                IngestStatus = MediaIngestStatus.Ready,
            };
            await _repository.AddMediaAssetAsync(asset);

            File.Delete(job.SourceFilePath);

            job.Status = IngestJobStatus.Completed;
            job.Progress = 100;
            job.CompletedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            job.Status = IngestJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            await _repository.SaveChangesAsync();
        }
    }

    private async Task<bool> IsCancelledAsync(Guid jobId)
    {
        var job = await _repository.GetJobByIdAsync(jobId);
        return job?.Status == IngestJobStatus.Cancelled;
    }

    private static void CleanupPartialFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best-effort cleanup, ignore */ }
    }

    public Task<List<MediaAsset>> GetAvailableAssetsAsync() => _repository.GetAllReadyAssetsAsync();

    public Task<List<MediaAsset>> GetUnattachedAssetsAsync() => _repository.GetReadyUnattachedAssetsAsync();

    public async Task AttachMediaToStoryAsync(Guid storyId, Guid mediaAssetId, string? role)
    {
        await _repository.AddStoryMediaAsync(new StoryMedia
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            MediaAssetId = mediaAssetId,
            Role = role,
        });
        await _repository.SaveChangesAsync();
    }

    public Task<List<StoryMedia>> GetStoryMediaAsync(Guid storyId) => _repository.GetMediaForStoryAsync(storyId);

    public Task<List<MediaAsset>> GetAssetsByIdsAsync(List<Guid> ids) => _repository.GetAssetsByIdsAsync(ids);
}