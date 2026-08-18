using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly InsertDbContext _context;

    public AssignmentRepository(InsertDbContext context)
    {
        _context = context;
    }

    public Task<Assignment?> GetByStoryIdAsync(Guid storyId) =>
        _context.Assignments.FirstOrDefaultAsync(a => a.StoryId == storyId);

    public async Task AddAsync(Assignment assignment) =>
        await _context.Assignments.AddAsync(assignment);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    public Task<List<Assignment>> GetAllAsync() =>
        _context.Assignments.ToListAsync();

    public Task<List<Assignment>> GetByReporterIdAsync(Guid reporterId) =>
        _context.Assignments.Where(a => a.ReporterId == reporterId).ToListAsync();
}