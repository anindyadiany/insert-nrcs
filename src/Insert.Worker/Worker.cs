using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var ingestService = scope.ServiceProvider.GetRequiredService<IngestService>();
                var queue = await ingestService.GetQueueAsync();
                var nextJob = queue.FirstOrDefault(j => j.Status == IngestJobStatus.Queued);

                if (nextJob is not null)
                {
                    logger.LogInformation("Processing ingest job {JobId}", nextJob.Id);
                    await ingestService.ProcessJobAsync(nextJob.Id);
                }
            }

            await Task.Delay(3000, stoppingToken);
        }
    }
}