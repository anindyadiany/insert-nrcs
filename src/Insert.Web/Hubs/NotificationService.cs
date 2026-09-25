using Insert.Application.Stories;
using Microsoft.AspNetCore.SignalR;

namespace Insert.Web.Hubs;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NrcsHub> _hub;

    public NotificationService(IHubContext<NrcsHub> hub) => _hub = hub;

    public Task RundownChangedAsync(Guid rundownId) =>
        _hub.Clients.Group($"rundown-{rundownId}").SendAsync("RundownChanged", rundownId);

    public Task AssignmentChangedAsync(Guid reporterId) =>
        _hub.Clients.Group($"user-{reporterId}").SendAsync("AssignmentChanged");

    public Task StoryStatusChangedAsync(Guid storyId, Guid? reporterId)
    {
        var tasks = new List<Task>
        {
            _hub.Clients.Group("role-Producer").SendAsync("StoryStatusChanged", storyId)
        };
        if (reporterId is Guid rid)
            tasks.Add(_hub.Clients.Group($"user-{rid}").SendAsync("StoryStatusChanged", storyId));
        return Task.WhenAll(tasks);
    }

    public Task ScriptVersionSavedAsync(Guid storyId, Guid? reporterId)
    {
        var tasks = new List<Task>
        {
            _hub.Clients.Group("role-Producer").SendAsync("ScriptVersionSaved", storyId)
        };
        if (reporterId is Guid rid)
            tasks.Add(_hub.Clients.Group($"user-{rid}").SendAsync("ScriptVersionSaved", storyId));
        return Task.WhenAll(tasks);
    }
}