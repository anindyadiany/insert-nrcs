using Insert.Domain.Entities;

namespace Insert.Application.Stories;

public class CreateAssignmentRequest
{
    public Guid StoryId { get; set; }
    public Guid ReporterId { get; set; }
    public string? Location { get; set; }
    public string? Event { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Brief { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAssignmentRequest
{
    public Guid ReporterId { get; set; }
    public string? Location { get; set; }
    public string? Event { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Brief { get; set; }
    public string? Notes { get; set; }
}

public class AssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly StoryWorkflowService _workflow;
    private readonly IUserLookupService _userLookup;

    public AssignmentService(
        IAssignmentRepository assignmentRepository,
        IStoryRepository storyRepository,
        StoryWorkflowService workflow,
        IUserLookupService userLookup)
    {
        _assignmentRepository = assignmentRepository;
        _storyRepository = storyRepository;
        _workflow = workflow;
        _userLookup = userLookup;
    }
    

    public Task<List<UserSummary>> GetReportersAsync() => _userLookup.GetUsersInRoleAsync("Reporter");

    public Task<Assignment?> GetAssignmentForStoryAsync(Guid storyId) => _assignmentRepository.GetByStoryIdAsync(storyId);

    public async Task<Assignment> CreateAssignmentAsync(CreateAssignmentRequest request)
    {
        var story = await _storyRepository.GetByIdAsync(request.StoryId)
            ?? throw new KeyNotFoundException("Story not found.");

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            StoryId = request.StoryId,
            ReporterId = request.ReporterId,
            Location = request.Location,
            Event = request.Event,
            Deadline = request.Deadline,
            Brief = request.Brief,
            Notes = request.Notes,
            Status = AssignmentStatus.Pending,
        };

        await _assignmentRepository.AddAsync(assignment);

        story.ReporterId = request.ReporterId;
        if (story.Status == StoryStatus.Draft)
        {
            _workflow.Transition(story, StoryStatus.Assigned);
        }

        await _assignmentRepository.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAssignmentAsync(Guid assignmentId, UpdateAssignmentRequest request)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new KeyNotFoundException("Assignment not found.");

        assignment.ReporterId = request.ReporterId;
        assignment.Location = request.Location;
        assignment.Event = request.Event;
        assignment.Deadline = request.Deadline;
        assignment.Brief = request.Brief;
        assignment.Notes = request.Notes;

        await _assignmentRepository.SaveChangesAsync();
    }

    public Task<List<Assignment>> GetAllAssignmentsAsync() => _assignmentRepository.GetAllAsync();
    public Task<List<Assignment>> GetAssignmentsForReporterAsync(Guid reporterId) => _assignmentRepository.GetByReporterIdAsync(reporterId);
}