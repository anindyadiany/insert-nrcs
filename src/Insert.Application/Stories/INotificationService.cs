namespace Insert.Application.Stories;

public interface INotificationService
{
    Task RundownChangedAsync(Guid rundownId);
    Task AssignmentChangedAsync(Guid reporterId);
    Task StoryStatusChangedAsync(Guid storyId, Guid? reporterId);
    Task ScriptVersionSavedAsync(Guid storyId, Guid? reporterId);
}