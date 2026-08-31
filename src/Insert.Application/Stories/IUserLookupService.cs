namespace Insert.Application.Stories;

public class UserSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IUserLookupService
{
    Task<List<UserSummary>> GetUsersInRoleAsync(string role);
    Task<List<UserSummary>> GetUsersByIdsAsync(IEnumerable<Guid> ids);
}
