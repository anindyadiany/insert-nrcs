using Microsoft.EntityFrameworkCore;
using Insert.Application.Stories;
using Insert.Domain.Entities;

namespace Insert.Infrastructure.Stories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly InsertDbContext _context;
    public AuditLogRepository(InsertDbContext context) => _context = context;

    public async Task AddAsync(AuditLog entry) => await _context.AuditLogs.AddAsync(entry);
    public Task<List<AuditLog>> GetRecentAsync(int count) =>
        _context.AuditLogs.OrderByDescending(a => a.Timestamp).Take(count).ToListAsync();

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}