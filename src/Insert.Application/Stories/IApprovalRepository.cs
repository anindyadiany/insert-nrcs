using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IApprovalRepository
{
    Task AddAsync(Approval approval);
    Task<List<Approval>> GetForStoryAsync(Guid storyId);
    Task SaveChangesAsync();
}