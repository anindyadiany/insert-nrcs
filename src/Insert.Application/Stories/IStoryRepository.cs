using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IStoryRepository
{
    Task<Story?> GetByIdAsync(Guid id);
    Task<List<Story>> GetAllAsync();
    Task AddAsync(Story story);
    Task SaveChangesAsync();
}