using Microsoft.AspNetCore.Identity;
using Insert.Application.Stories;
using Insert.Infrastructure.Identity;

namespace Insert.Infrastructure.Stories;

public class UserLookupService : IUserLookupService
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UserLookupService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<List<UserSummary>> GetUsersInRoleAsync(string role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        return users.Select(u => new UserSummary { Id = u.Id, Name = u.Name }).ToList();
    }

    public async Task<List<UserSummary>> GetUsersByIdsAsync(IEnumerable<Guid> ids)
    {
        var idSet = ids.Distinct().ToList();
        var result = new List<UserSummary>();
        foreach (var id in idSet)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is not null)
                result.Add(new UserSummary { Id = user.Id, Name = user.Name });
        }
        return result;
    }
}