using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class ApprovalService
{
    private readonly IApprovalRepository _repository;
    private readonly IScriptRepository _scriptRepository;
    private readonly StoryService _storyService;

    public ApprovalService(
        IApprovalRepository repository,
        IScriptRepository scriptRepository,
        StoryService storyService)
    {
        _repository = repository;
        _scriptRepository = scriptRepository;
        _storyService = storyService;
    }

    public Task<List<Approval>> GetHistoryAsync(Guid storyId) => _repository.GetForStoryAsync(storyId);

    public async Task SubmitDecisionAsync(Guid storyId, Guid reviewerId, ApprovalDecision decision, string? comment)
    {
        // capture which script version was actually reviewed
        Guid? scriptVersionId = null;
        var script = await _scriptRepository.GetByStoryIdAsync(storyId);
        if (script is not null)
        {
            var latest = await _scriptRepository.GetLatestVersionAsync(script.Id);
            scriptVersionId = latest?.Id;
        }

        await _repository.AddAsync(new Approval
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            ScriptVersionId = scriptVersionId,
            ReviewerId = reviewerId,
            Decision = decision,
            Comment = comment,
        });
        await _repository.SaveChangesAsync();

        var newStatus = decision switch
        {
            ApprovalDecision.Approved => StoryStatus.Approved,
            ApprovalDecision.Rejected => StoryStatus.InProgress,
            _ => (StoryStatus?)null
        };

        if (newStatus is not null)
        {
            await _storyService.ChangeStatusAsync(storyId, newStatus.Value, reviewerId);
        }
    }
}