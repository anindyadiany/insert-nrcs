using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IRundownRepository
{
    Task<Rundown?> GetLatestAsync();
    Task<Rundown?> GetByIdAsync(Guid id);
    Task AddAsync(Rundown rundown);

    Task<List<RundownItem>> GetItemsAsync(Guid rundownId);
    Task AddItemAsync(RundownItem item);
    Task<RundownItem?> GetItemAsync(Guid itemId);
    Task RemoveItemAsync(RundownItem item);

    Task SaveChangesAsync();
}