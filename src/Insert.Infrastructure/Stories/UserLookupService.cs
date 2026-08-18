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
}