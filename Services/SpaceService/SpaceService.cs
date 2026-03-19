using AI_genda_API.Contracts.Space;
using AI_genda_API.Entities;
using AI_genda_API.Entities.Enums;
using AI_genda_API.Errors;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace AI_genda_API.Services.SpaceService;

public class SpaceService(AppContext context, IHttpContextAccessor httpContextAccessor) : ISpaceService
{
    private readonly AppContext _Context = context;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    public async Task<Result<SpaceDetailResponse>> AddAsync(int WorkspaceId, string UserId, SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var workspaceExists = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!workspaceExists)
            return Result.Faluire<SpaceDetailResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var isMemberOrOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken)
            || await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!isMemberOrOwner)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);

        var space = new Space
        {
            Name = request.Name,
            Descreption = request.Description,
            IconHexa = request.IconHexa,
            IsPublic = request.IsPublic,
            WorkSpaceId = WorkspaceId,
            CreatedById = UserId,
            LastActivity = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _Context.Spaces.AddAsync(space, cancellationToken);
        await _Context.SaveChangesAsync(cancellationToken);

        var response = new SpaceDetailResponse(
            space.Id,
            space.Name,
            space.Descreption,
            space.IconHexa,
            space.LastActivity,
            space.IsPublic,
            0,
            0
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<SpaceDetailResponse>>> GetAllAsync(int WorkspaceId, string UserId, CancellationToken cancellationToken = default!)
    {
        var workspaceExists = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!workspaceExists)
            return Result.Faluire<IEnumerable<SpaceDetailResponse>>(WorkSpaceErrors.WorkSpaceNotFound);

        var isMemberOrOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken)
            || await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!isMemberOrOwner)
            return Result.Faluire<IEnumerable<SpaceDetailResponse>>(WorkspaceMemberErrors.AccessDenied);

        var spaces = await _Context.Spaces
            .Where(s => s.WorkSpaceId == WorkspaceId && s.IsActive)
            .Select(s => new SpaceDetailResponse(
                s.Id,
                s.Name,
                s.Descreption,
                s.IconHexa,
                s.LastActivity,
                s.IsPublic,
                s.Tasks.Count(t => t.RemovedAt == null),
                s.Tasks.Count(t => t.RemovedAt == null && t.Status == TaskStatuss.Completed)
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SpaceDetailResponse>>(spaces);
    }

    public async System.Threading.Tasks.Task<Result<SpaceDetailResponse>> GetByIdAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var isMemberOrOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken)
            || await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!isMemberOrOwner)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);

        var space = await _Context.Spaces
            .Where(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.IsActive)
            .Select(s => new SpaceDetailResponse(
                s.Id,
                s.Name,
                s.Descreption,
                s.IconHexa,
                s.LastActivity,
                s.IsPublic,
                s.Tasks.Count(t => t.RemovedAt == null),
                s.Tasks.Count(t => t.RemovedAt == null && t.Status == TaskStatuss.Completed)
            ))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (space is null)
            return Result.Faluire<SpaceDetailResponse>(SpaceErrors.SpaceNotFound);

        return Result.Success(space);
    }

    public async System.Threading.Tasks.Task<Result<SpaceDetailResponse>> UpdateAsync(int WorkspaceId, string Id, string UserId, SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var isOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        if (!isOwner)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);

        var space = await _Context.Spaces
            .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.IsActive, cancellationToken);

        if (space is null)
            return Result.Faluire<SpaceDetailResponse>(SpaceErrors.SpaceNotFound);

        space.Name = request.Name;
        space.Descreption = request.Description;
        space.IconHexa = request.IconHexa;
        space.IsPublic = request.IsPublic;
        space.LastActivity = DateOnly.FromDateTime(DateTime.UtcNow);
        space.UpdatedById = UserId;
        space.UpdatedAt = DateTime.UtcNow;

        await _Context.SaveChangesAsync(cancellationToken);

        var response = new SpaceDetailResponse(
            space.Id,
            space.Name,
            space.Descreption,
            space.IconHexa,
            space.LastActivity,
            space.IsPublic,
            0,
            0
        );

        return Result.Success(response);
    }

    public async System.Threading.Tasks.Task<Result> DeleteAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var isOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        if (!isOwner)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var space = await _Context.Spaces
            .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.IsActive, cancellationToken);

        if (space is null)
            return Result.Faluire(SpaceErrors.SpaceNotFound);

        space.IsActive = false;
        space.RemovedAt = DateTime.UtcNow;
        space.RemovedById = UserId;

        await _Context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async System.Threading.Tasks.Task<Result> RestoreAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var isOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        if (!isOwner)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var space = await _Context.Spaces
            .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && !s.IsActive, cancellationToken);

        if (space is null)
            return Result.Faluire(SpaceErrors.SpaceNotFound);

        space.IsActive = true;
        space.RemovedAt = null;
        space.RemovedById = null;

        await _Context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}