using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry);
    Task SaveChangesAsync();
}

public class AuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task LogAsync(Guid userId, string action, string entityType, Guid entityId, string? before = null, string? after = null)
    {
        await _repository.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeValue = before,
            AfterValue = after,
        });
        await _repository.SaveChangesAsync();
    }
}