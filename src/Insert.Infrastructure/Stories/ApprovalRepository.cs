using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly InsertDbContext _context;
    public ApprovalRepository(InsertDbContext context) => _context = context;

    public async Task AddAsync(Approval approval) => await _context.Approvals.AddAsync(approval);

    public Task<List<Approval>> GetForStoryAsync(Guid storyId) =>
        _context.Approvals.Where(a => a.StoryId == storyId)
                          .OrderByDescending(a => a.CreatedAt)
                          .ToListAsync();

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}