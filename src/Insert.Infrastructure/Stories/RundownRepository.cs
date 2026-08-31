using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class RundownRepository : IRundownRepository
{
    private readonly InsertDbContext _context;
    public RundownRepository(InsertDbContext context) => _context = context;

    public Task<Rundown?> GetLatestAsync() =>
        _context.Rundowns.OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();

    public Task<Rundown?> GetByIdAsync(Guid id) =>
        _context.Rundowns.FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(Rundown rundown) => await _context.Rundowns.AddAsync(rundown);

    public Task<List<RundownItem>> GetItemsAsync(Guid rundownId) =>
        _context.RundownItems.Where(i => i.RundownId == rundownId)
                              .OrderBy(i => i.SortOrder)
                              .ToListAsync();

    public async Task AddItemAsync(RundownItem item) => await _context.RundownItems.AddAsync(item);

    public Task<RundownItem?> GetItemAsync(Guid itemId) =>
        _context.RundownItems.FirstOrDefaultAsync(i => i.Id == itemId);

    public Task RemoveItemAsync(RundownItem item)
    {
        _context.RundownItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
