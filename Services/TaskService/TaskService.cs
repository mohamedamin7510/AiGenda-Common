
namespace AI_genda_API.Services.TaskService;

public class TaskService(AppContext context, IHttpContextAccessor httpContextAccessor) : ITaskService
{
    private readonly AppContext _Context = context;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    private async Task<bool> HasAccessAsync(int WorkspaceId, string UserId, CancellationToken cancellationToken)
    {
        return await _Context.WorkSpaces.AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken)
            || await _Context.WorkspaceMembers.AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);
    }

    public async Task<Result<TaskResponse>> AddAsync(int WorkspaceId, string SpaceId, string UserId, TaskRequest request, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire<TaskResponse>(WorkspaceMemberErrors.AccessDenied);

        var spaceExists = await _Context.Spaces
            .AnyAsync(s => s.Id == SpaceId && s.WorkSpaceId == WorkspaceId && s.IsActive, cancellationToken);

        if (!spaceExists)
            return Result.Faluire<TaskResponse>(SpaceErrors.SpaceNotFound);

        var task = new Entities.Task
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            SpaceId = SpaceId,
            CreatedById = UserId
        };

        await _Context.Tasks.AddAsync(task, cancellationToken);
        await _Context.SaveChangesAsync(cancellationToken);

        var response = new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.CreatedAt,
            []
        );

        return Result.Success(response);
    }

    public async System.Threading.Tasks.Task<Result<IEnumerable<TaskResponse>>> GetAllAsync(int WorkspaceId, string SpaceId, string UserId, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire<IEnumerable<TaskResponse>>(WorkspaceMemberErrors.AccessDenied);

        var spaceExists = await _Context.Spaces
            .AnyAsync(s => s.Id == SpaceId && s.WorkSpaceId == WorkspaceId && s.IsActive, cancellationToken);

        if (!spaceExists)
            return Result.Faluire<IEnumerable<TaskResponse>>(SpaceErrors.SpaceNotFound);

        var tasks = await _Context.Tasks
            .Where(t => t.SpaceId == SpaceId && t.RemovedAt == null)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.CreatedAt,
                t.TaskAssignees.Select(a => new AssigneeResponse(
                    a.UserId,
                    a.User!.FirstName + " " + a.User.SecondName,
                    a.User.AvatarUrl
                )).ToList()
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<TaskResponse>>(tasks);
    }

    public async System.Threading.Tasks.Task<Result<TaskResponse>> GetByIdAsync(int WorkspaceId, string SpaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire<TaskResponse>(WorkspaceMemberErrors.AccessDenied);

        var task = await _Context.Tasks
            .Where(t => t.Id == Id && t.SpaceId == SpaceId && t.RemovedAt == null)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.CreatedAt,
                t.TaskAssignees.Select(a => new AssigneeResponse(
                    a.UserId,
                    a.User!.FirstName + " " + a.User.SecondName,
                    a.User.AvatarUrl
                )).ToList()
            ))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (task is null)
            return Result.Faluire<TaskResponse>(TaskErrors.TaskNotFound);

        return Result.Success(task);
    }

    public async System.Threading.Tasks.Task<Result<TaskResponse>> UpdateAsync(int WorkspaceId, string SpaceId, string Id, string UserId, TaskRequest request, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire<TaskResponse>(WorkspaceMemberErrors.AccessDenied);

        var task = await _Context.Tasks
            .SingleOrDefaultAsync(t => t.Id == Id && t.SpaceId == SpaceId && t.RemovedAt == null, cancellationToken);

        if (task is null)
            return Result.Faluire<TaskResponse>(TaskErrors.TaskNotFound);

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedById = UserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _Context.SaveChangesAsync(cancellationToken);

        var response = new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.CreatedAt,
            []
        );

        return Result.Success(response);
    }

    public async System.Threading.Tasks.Task<Result> UpdateStatusAsync(int WorkspaceId, string SpaceId, string Id, string UserId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var task = await _Context.Tasks
            .SingleOrDefaultAsync(t => t.Id == Id && t.SpaceId == SpaceId && t.RemovedAt == null, cancellationToken);

        if (task is null)
            return Result.Faluire(TaskErrors.TaskNotFound);

        task.Status = request.Status;
        task.UpdatedById = UserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _Context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async System.Threading.Tasks.Task<Result> DeleteAsync(int WorkspaceId, string SpaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var task = await _Context.Tasks
            .SingleOrDefaultAsync(t => t.Id == Id && t.SpaceId == SpaceId && t.RemovedAt == null, cancellationToken);

        if (task is null)
            return Result.Faluire(TaskErrors.TaskNotFound);

        task.RemovedAt = DateTime.UtcNow;
        task.RemovedById = UserId;

        await _Context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async System.Threading.Tasks.Task<Result> RestoreAsync(int WorkspaceId, string SpaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var task = await _Context.Tasks
            .SingleOrDefaultAsync(t => t.Id == Id && t.SpaceId == SpaceId && t.RemovedAt != null, cancellationToken);

        if (task is null)
            return Result.Faluire(TaskErrors.TaskNotFound);

        task.RemovedAt = null;
        task.RemovedById = null;

        await _Context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async System.Threading.Tasks.Task<Result> AssignMemberAsync(int WorkspaceId, string SpaceId, string Id, string UserId, AssignTaskRequest request, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var taskExists = await _Context.Tasks
            .AnyAsync(t => t.Id == Id && t.SpaceId == SpaceId && t.RemovedAt == null, cancellationToken);

        if (!taskExists)
            return Result.Faluire(TaskErrors.TaskNotFound);

        var assignedUser = await _Context.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (assignedUser is null)
            return Result.Faluire(UserErrors.EmailnotFounded);

        var isMember = await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == assignedUser.Id, cancellationToken)
            || await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == assignedUser.Id, cancellationToken);

        if (!isMember)
            return Result.Faluire(WorkspaceMemberErrors.MemberNotFounded);

        var alreadyAssigned = await _Context.TaskAssignees
            .AnyAsync(a => a.TaskId == Id && a.UserId == assignedUser.Id, cancellationToken);

        if (alreadyAssigned)
            return Result.Faluire(TaskErrors.AlreadyAssigned);

        var assignee = new TaskAssignee
        {
            TaskId = Id,
            UserId = assignedUser.Id,
            AssignedAt = DateTime.UtcNow
        };

        await _Context.TaskAssignees.AddAsync(assignee, cancellationToken);
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async System.Threading.Tasks.Task<Result> UnAssignMemberAsync(int WorkspaceId, string SpaceId, string Id, string UserId, AssignTaskRequest request, CancellationToken cancellationToken = default!)
    {
        var hasAccess = await HasAccessAsync(WorkspaceId, UserId, cancellationToken);
        if (!hasAccess)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var assignedUser = await _Context.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (assignedUser is null)
            return Result.Faluire(UserErrors.EmailnotFounded);

        var assignee = await _Context.TaskAssignees
            .SingleOrDefaultAsync(a => a.TaskId == Id && a.UserId == assignedUser.Id, cancellationToken);

        if (assignee is null)
            return Result.Faluire(TaskErrors.AssigneeNotFound);

        _Context.TaskAssignees.Remove(assignee);
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}