using Microsoft.AspNetCore.SignalR;

namespace Insert.Web.Hubs;

public class NrcsHub : Hub
{
    public Task JoinRundownGroup(Guid rundownId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"rundown-{rundownId}");

    public Task LeaveRundownGroup(Guid rundownId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"rundown-{rundownId}");

    public Task JoinUserGroup(Guid userId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

    public Task JoinRoleGroup(string roleName) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"role-{roleName}");

}
