using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class CreateRundownRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Program { get; set; }
    public DateTime AirDate { get; set; } = DateTime.UtcNow.Date;
    public TimeSpan StartTime { get; set; } = new TimeSpan(18, 0, 0);
}

public class AddSegmentRequest
{
    public RundownItemType ItemType { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
}

public class RundownItemView
{
    public Guid Id { get; set; }
    public RundownItemType ItemType { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubText { get; set; }
    public int DurationSeconds { get; set; }
    public int StartSeconds { get; set; }
    public int EndSeconds { get; set; }
    public Guid? StoryId { get; set; }
    public StoryStatus? StoryStatus { get; set; }
}

public class StoryPickerItem
{
    public Guid StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ReporterName { get; set; }
    public string? Category { get; set; }
    public int DurationSeconds { get; set; }
}

public class RundownBoard
{
    public Rundown Rundown { get; set; } = null!;
    public List<RundownItemView> Items { get; set; } = new();
    public List<StoryPickerItem> AvailableApprovedStories { get; set; } = new();
    public int TotalDurationSeconds { get; set; }
    public int OnAirEndSeconds { get; set; }
}

public class RundownService
{
    private readonly IRundownRepository _repository;
    private readonly IStoryRepository _storyRepository;
    private readonly IScriptRepository _scriptRepository;
    private readonly IUserLookupService _userLookup;
    private readonly AuditLogService _auditLog;

    public RundownService(
        IRundownRepository repository,
        IStoryRepository storyRepository,
        IScriptRepository scriptRepository,
        IUserLookupService userLookup,
        AuditLogService auditLog)
    {
        _repository = repository;
        _storyRepository = storyRepository;
        _scriptRepository = scriptRepository;
        _userLookup = userLookup;
        _auditLog = auditLog;
    }

    public async Task<Rundown> CreateRundownAsync(CreateRundownRequest request, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        var rundown = new Rundown
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Program = request.Program,
            AirDate = request.AirDate,
            StartTime = request.StartTime,
        };

        await _repository.AddAsync(rundown);
        await _repository.SaveChangesAsync();
        await _auditLog.LogAsync(userId, "RundownCreated", "Rundown", rundown.Id, null, rundown.Title);
        return rundown;
    }

    public async Task<RundownBoard?> GetBoardAsync(Guid? rundownId)
    {
        var rundown = rundownId is Guid id
            ? await _repository.GetByIdAsync(id)
            : await _repository.GetLatestAsync();

        if (rundown is null) return null;

        var items = await _repository.GetItemsAsync(rundown.Id);
        items = items.OrderBy(i => i.SortOrder).ToList();

        var storyIds = items.Where(i => i.ItemType == RundownItemType.Story && i.StoryId is not null)
                             .Select(i => i.StoryId!.Value)
                             .ToList();

        var storyLookup = new Dictionary<Guid, Story>();
        var durationLookup = new Dictionary<Guid, int>();
        var reporterLookup = new Dictionary<Guid, string>();

        foreach (var storyId in storyIds)
        {
            var story = await _storyRepository.GetByIdAsync(storyId);
            if (story is null) continue;
            storyLookup[storyId] = story;

            var script = await _scriptRepository.GetByStoryIdAsync(storyId);
            var latestVersion = script is not null ? await _scriptRepository.GetLatestVersionAsync(script.Id) : null;
            durationLookup[storyId] = latestVersion?.EstimatedDurationSeconds ?? 0;
        }

        var reporterIds = storyLookup.Values.Where(s => s.ReporterId is not null).Select(s => s.ReporterId!.Value).Distinct();
        var reporters = await _userLookup.GetUsersByIdsAsync(reporterIds);
        foreach (var r in reporters) reporterLookup[r.Id] = r.Name;

        var cursor = (int)rundown.StartTime.TotalSeconds;
        var views = new List<RundownItemView>();

        foreach (var item in items)
        {
            int duration;
            string title;
            string? subText = null;
            StoryStatus? status = null;

            if (item.ItemType == RundownItemType.Story && item.StoryId is Guid sid && storyLookup.TryGetValue(sid, out var story))
            {
                duration = durationLookup.GetValueOrDefault(sid, 0);
                title = story.Title;
                status = story.Status;
                var reporterName = story.ReporterId is Guid rid && reporterLookup.TryGetValue(rid, out var name) ? name : null;
                subText = string.Join(" · ", new[] { reporterName is not null ? $"Reporter: {reporterName}" : null, story.Category is not null ? $"Category: {story.Category}" : null }
                    .Where(x => x is not null));
            }
            else
            {
                duration = item.SegmentDurationSeconds ?? 0;
                title = item.SegmentLabel ?? item.ItemType.ToString();
            }

            var start = cursor;
            var end = cursor + duration;
            cursor = end;

            views.Add(new RundownItemView
            {
                Id = item.Id,
                ItemType = item.ItemType,
                SortOrder = item.SortOrder,
                Title = title,
                SubText = subText,
                DurationSeconds = duration,
                StartSeconds = start,
                EndSeconds = end,
                StoryId = item.StoryId,
                StoryStatus = status,
            });
        }

        var pickerExcludeIds = storyIds.ToHashSet();
        var allStories = await _storyRepository.GetAllAsync();
        var approvedStories = allStories.Where(s => s.Status == StoryStatus.Approved && !pickerExcludeIds.Contains(s.Id)).ToList();
        var pickerReporterIds = approvedStories.Where(s => s.ReporterId is not null).Select(s => s.ReporterId!.Value).Distinct();
        var pickerReporters = (await _userLookup.GetUsersByIdsAsync(pickerReporterIds)).ToDictionary(r => r.Id, r => r.Name);

        var picker = new List<StoryPickerItem>();
        foreach (var story in approvedStories)
        {
            var script = await _scriptRepository.GetByStoryIdAsync(story.Id);
            var latestVersion = script is not null ? await _scriptRepository.GetLatestVersionAsync(script.Id) : null;
            picker.Add(new StoryPickerItem
            {
                StoryId = story.Id,
                Title = story.Title,
                ReporterName = story.ReporterId is Guid rid2 && pickerReporters.TryGetValue(rid2, out var n) ? n : null,
                Category = story.Category,
                DurationSeconds = latestVersion?.EstimatedDurationSeconds ?? 0,
            });
        }

        var total = views.Sum(v => v.DurationSeconds);

        return new RundownBoard
        {
            Rundown = rundown,
            Items = views,
            AvailableApprovedStories = picker,
            TotalDurationSeconds = total,
            OnAirEndSeconds = (int)rundown.StartTime.TotalSeconds + total,
        };
    }

    public async Task AddStoryAsync(Guid rundownId, Guid storyId, Guid userId)
    {
        var story = await _storyRepository.GetByIdAsync(storyId)
            ?? throw new KeyNotFoundException("Story not found.");

        if (story.Status != StoryStatus.Approved)
            throw new InvalidOperationException("Only Approved stories can be added to the rundown.");

        var existingItems = await _repository.GetItemsAsync(rundownId);
        if (existingItems.Any(i => i.StoryId == storyId))
            throw new InvalidOperationException("This story is already in the rundown.");

        var item = new RundownItem
        {
            Id = Guid.NewGuid(),
            RundownId = rundownId,
            ItemType = RundownItemType.Story,
            StoryId = storyId,
            SortOrder = existingItems.Count == 0 ? 0 : existingItems.Max(i => i.SortOrder) + 1,
        };

        await _repository.AddItemAsync(item);
        await _repository.SaveChangesAsync();
        await _auditLog.LogAsync(userId, "StoryAddedToRundown", "Rundown", rundownId, null, story.Title);
    }

    public async Task AddSegmentAsync(Guid rundownId, AddSegmentRequest request, Guid userId)
    {
        if (request.ItemType == RundownItemType.Story)
            throw new ArgumentException("Use AddStoryAsync for story items.");

        var existingItems = await _repository.GetItemsAsync(rundownId);

        var item = new RundownItem
        {
            Id = Guid.NewGuid(),
            RundownId = rundownId,
            ItemType = request.ItemType,
            SegmentLabel = string.IsNullOrWhiteSpace(request.Label) ? request.ItemType.ToString() : request.Label.Trim(),
            SegmentDurationSeconds = Math.Max(0, request.DurationSeconds),
            SortOrder = existingItems.Count == 0 ? 0 : existingItems.Max(i => i.SortOrder) + 1,
        };

        await _repository.AddItemAsync(item);
        await _repository.SaveChangesAsync();
        await _auditLog.LogAsync(userId, "SegmentAddedToRundown", "Rundown", rundownId, null, item.SegmentLabel);
    }

    public async Task RemoveItemAsync(Guid rundownId, Guid itemId, Guid userId)
    {
        var item = await _repository.GetItemAsync(itemId)
            ?? throw new KeyNotFoundException("Rundown item not found.");

        if (item.RundownId != rundownId)
            throw new InvalidOperationException("Item does not belong to this rundown.");

        await _repository.RemoveItemAsync(item);
        await _repository.SaveChangesAsync();
        await _auditLog.LogAsync(userId, "RundownItemRemoved", "Rundown", rundownId, item.SegmentLabel ?? item.StoryId?.ToString(), null);
    }

    public async Task ReorderAsync(Guid rundownId, List<Guid> orderedItemIds)
    {
        var items = await _repository.GetItemsAsync(rundownId);
        var lookup = items.ToDictionary(i => i.Id);

        for (var i = 0; i < orderedItemIds.Count; i++)
        {
            if (lookup.TryGetValue(orderedItemIds[i], out var item))
                item.SortOrder = i;
        }

        await _repository.SaveChangesAsync();
    }
}
