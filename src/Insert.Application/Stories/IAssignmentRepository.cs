using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByStoryIdAsync(Guid storyId);
    Task<Assignment?> GetByIdAsync(Guid id);
    Task AddAsync(Assignment assignment);
    Task SaveChangesAsync();
    Task<List<Assignment>> GetAllAsync();
    Task<List<Assignment>> GetByReporterIdAsync(Guid reporterId);
}