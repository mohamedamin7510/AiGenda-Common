using AI_genda_API.Contracts.Ai;
using AI_genda_API.Entities;
using AI_genda_API.Presistience;
using Microsoft.EntityFrameworkCore;

namespace AI_genda_API.Services.Ai;

public class AiPersistenceService(AppContext context) : IAiPersistenceService
{
    private readonly AppContext _context = context;

    public async System.Threading.Tasks.Task<AgentTreeRequest> PersistAgentTreeAsync(string userId, AgentTreeRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var workspace = new WorkSpace
        {
            Name = request.Workspace.Name,
            Description = request.Workspace.Description,
            IconCode = null,
            IsActive = true,
            Visibility = Abstractions.Enums.WorkSpaceVisibility.Private,
            CreatedById = userId,
            CreatedAt = now
        };

        foreach (var spacePayload in request.Workspace.Spaces)
        {
            var space = new Space
            {
                Name = spacePayload.Name,
                Description = spacePayload.Description,
                IconCode = spacePayload.IconCode ?? string.Empty,
                IsActive = true,
                IsPublic = true,
                LastActivity = DateOnly.FromDateTime(now),
                CreatedById = userId,
                CreatedAt = now
            };

            foreach (var taskPayload in spacePayload.Tasks)
            {
                var task = new Entities.Task
                {
                    Title = taskPayload.Title,
                    Description = taskPayload.Description,
                    DueDate = taskPayload.DueDate,
                    IsActive = true,
                    CreatedById = userId,
                    CreatedAt = now
                };

                foreach (var subTaskPayload in taskPayload.Subtasks)
                {
                    var subTask = new SubTask
                    {
                        Title = subTaskPayload.Title,
                        IsCompleted = subTaskPayload.IsCompleted,
                        CreatedById = userId,
                        CreatedAt = now
                    };

                    task.SubTasks.Add(subTask);
                }

                space.Tasks.Add(task);
            }

            workspace.Spaces.Add(space);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.WorkSpaces.AddAsync(workspace, cancellationToken);

            var membership = new WorkspaceMember
            {
                UserID = userId,
                IsOwner = true,
                JoinedAt = now,
                Permissions = [],
                WorkSpaces = workspace
            };
            await _context.WorkspaceMembers.AddAsync(membership, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return request;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async System.Threading.Tasks.Task<FactoryResetResponse> FactoryResetAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var workspaceIds = await _context.WorkSpaces
                .Where(w => w.CreatedById == userId)
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            if (workspaceIds.Count == 0)
            {
                return new FactoryResetResponse(0, 0, 0, 0, 0);
            }

            var spaceIds = await _context.Spaces
                .Where(s => workspaceIds.Contains(s.WorkSpaceId))
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            var taskIds = await _context.Tasks
                .Where(t => spaceIds.Contains(t.SpaceId))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            var subTasks = await _context.SubTasks
                .Where(st => taskIds.Contains(st.TaskId))
                .ToListAsync(cancellationToken);
            _context.SubTasks.RemoveRange(subTasks);

            var tasks = await _context.Tasks
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
            _context.Tasks.RemoveRange(tasks);

            var spaces = await _context.Spaces
                .Where(s => spaceIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            _context.Spaces.RemoveRange(spaces);

            var members = await _context.WorkspaceMembers
                .Where(m => workspaceIds.Contains(m.WrokSpaceID))
                .ToListAsync(cancellationToken);
            _context.WorkspaceMembers.RemoveRange(members);

            var workspaces = await _context.WorkSpaces
                .Where(w => workspaceIds.Contains(w.Id))
                .ToListAsync(cancellationToken);
            _context.WorkSpaces.RemoveRange(workspaces);

            var subTaskCount = subTasks.Count;
            var taskCount = tasks.Count;
            var spaceCount = spaces.Count;
            var memberCount = members.Count;
            var workspaceCount = workspaces.Count;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new FactoryResetResponse(workspaceCount, spaceCount, taskCount, subTaskCount, memberCount);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
