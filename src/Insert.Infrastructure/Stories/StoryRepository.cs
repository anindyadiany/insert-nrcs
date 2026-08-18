using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class StoryRepository : IStoryRepository
{
    private readonly InsertDbContext _context;

    public StoryRepository(InsertDbContext context)
    {
        _context = context;
    }

    public Task<Story?> GetByIdAsync(Guid id) =>
        _context.Stories.FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Story>> GetAllAsync() =>
        _context.Stories.OrderByDescending(s => s.CreatedAt).ToListAsync();

    public async Task AddAsync(Story story) =>
        await _context.Stories.AddAsync(story);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}