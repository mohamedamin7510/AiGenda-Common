using AI_genda_API.Abstractions.Enums;

namespace AI_genda_API.Services.SpaceService;

public class SpaceService(AppContext context, IHttpContextAccessor httpContextAccessor) : ISpaceService
{
    private readonly AppContext _Context = context;

    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    public async Task<Result<SpaceDetailResponse>>AddAsync(int WorkspaceId, string UserId, SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var IsWorkspaceExists = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!IsWorkspaceExists)
            return Result.Faluire<SpaceDetailResponse>(WorkSpaceErrors.WorkSpaceNotFound);


        var IsMemberOrOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken)
            || await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);


        if (!IsMemberOrOwner)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);

        var space = new Space
        {
            Name = request.Name,
            Description = request.Description,
            IconCode = request.IconCode,
            IsPublic = request.IsPublic,
            WorkSpaceId = WorkspaceId,
            LastActivity = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _Context.Spaces.AddAsync(space, cancellationToken);

        await _Context.SaveChangesAsync(cancellationToken);

        var response = space.Adapt<Space, SpaceDetailResponse>();

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<SpaceDetailResponse>>> GetAllAsync(int WorkspaceId, string UserId, CancellationToken cancellationToken = default!)
    {
        var WorkspaceExists = await _Context.WorkSpaces
          .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!WorkspaceExists)
            return Result.Faluire<IEnumerable<SpaceDetailResponse>>(WorkSpaceErrors.WorkSpaceNotFound);

        var IsOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        var IsMember = !IsOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!IsOwner && !IsMember)
            return Result.Faluire<IEnumerable<SpaceDetailResponse>>(WorkspaceMemberErrors.AccessDenied);

        IEnumerable<SpaceDetailResponse> spaces = null; 

        if (IsOwner)
        {
           spaces  = await _Context.Spaces
              .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt == null)
              .Select(s => new SpaceDetailResponse(
                          s.Id,
                          s.Name,
                          s.Description,
                          s.IconCode,
                          s.LastActivity,
                          s.IsPublic,
                     s.Tasks.Count(t => t.RemovedAt == null),
                     s.Tasks.Count(t => t.RemovedAt == null && t.Status == TaskStatuss.Completed)
              ))
              .AsNoTracking()
              .ToListAsync(cancellationToken);
        }
        else
        {
             spaces = await _Context.Spaces
             .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt == null && s.IsPublic)
             .Select(s => new SpaceDetailResponse(
                         s.Id,
                         s.Name,
                         s.Description,
                         s.IconCode,
                         s.LastActivity,
                         s.IsPublic,
                    s.Tasks.Count(t => t.RemovedAt == null),
                    s.Tasks.Count(t => t.RemovedAt == null && t.Status == TaskStatuss.Completed)
             ))
             .AsNoTracking()
             .ToListAsync(cancellationToken);
        }

        return Result.Success(spaces);
    }

    public async Task<Result<SpaceDetailResponse>> GetByIdAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var WorkspaceExists = await _Context.WorkSpaces
          .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!WorkspaceExists)
            return Result.Faluire<SpaceDetailResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var IsOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        var IsMember = !IsOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!IsOwner && !IsMember)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);

        SpaceDetailResponse? Space = null;

        if (IsOwner)
        {
            Space = await _Context.Spaces
               .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt == null && s.Id == Id)
               .Select(s => new SpaceDetailResponse(
                           s.Id,
                           s.Name,
                           s.Description,
                           s.IconCode,
                           s.LastActivity,
                           s.IsPublic,
                      s.Tasks.Count(t => t.RemovedAt == null),
                      s.Tasks.Count(t => t.RemovedAt == null && t.Status == TaskStatuss.Completed)
               ))
               .AsNoTracking()
               .SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            Space = await _Context.Spaces
            .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt == null && s.Id == Id && s.IsPublic)
            .Select(s => new SpaceDetailResponse(
                        s.Id,
                        s.Name,
                        s.Description,
                        s.IconCode,
                        s.LastActivity,
                        s.IsPublic,
                   s.Tasks.Count(t => t.RemovedAt == null),
                   s.Tasks.Count(t => t.RemovedAt == null && t.Status == TaskStatuss.Completed)
            ))
            .AsNoTracking()
           .SingleOrDefaultAsync(cancellationToken);
        }

        if (Space is null )
        {
            return Result.Faluire<SpaceDetailResponse>(SpaceErrors.SpaceNotFound);
        }


        return Result.Success(Space!);
    }

    public async Task<Result<SpaceDetailResponse>> UpdateAsync(int WorkspaceId, string Id, string UserId, SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var WorkspaceExists = await _Context.WorkSpaces
          .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!WorkspaceExists)
            return Result.Faluire<SpaceDetailResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var IsOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        var IsMember = !IsOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!IsOwner && !IsMember)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);


        Space? Space = null;

        if (IsOwner)
        {
            Space = await _Context.Spaces
               .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt == null && s.Id == Id)
               .SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            Space = await _Context.Spaces
            .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt == null && s.Id == Id && s.IsPublic)
            .SingleOrDefaultAsync(cancellationToken);
        }


        if (Space is null)
            return Result.Faluire<SpaceDetailResponse>(SpaceErrors.SpaceNotFound);

        Space.Name = request.Name;
        Space.Description = request.Description;
        Space.IconCode = request.IconCode;
        Space.IsPublic = request.IsPublic;
        Space.LastActivity = DateOnly.FromDateTime(DateTime.UtcNow);

        await _Context.SaveChangesAsync(cancellationToken);

        var response =  Space.Adapt< Space, SpaceDetailResponse >();

        return Result.Success(response);
    }

    public async Task<Result> DeleteAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var WorkspaceExists = await _Context.WorkSpaces
          .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!WorkspaceExists)
            return Result.Faluire<SpaceDetailResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var IsOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        var IsMember = !IsOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!IsOwner && !IsMember)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);


        var space = await _Context.Spaces
            .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.RemovedAt == null , cancellationToken);

        if (space is null)
            return Result.Faluire(SpaceErrors.SpaceNotFound);

        space.IsActive = false;
        space.RemovedAt = DateTime.UtcNow;
        space.RemovedById = UserId;

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RestoreAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var WorkspaceExists = await _Context.WorkSpaces
          .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!WorkspaceExists)
            return Result.Faluire<SpaceDetailResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var IsOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        var IsMember = !IsOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!IsOwner && !IsMember)
            return Result.Faluire<SpaceDetailResponse>(WorkspaceMemberErrors.AccessDenied);


        var Space = await _Context.Spaces
            .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.RemovedAt != null, cancellationToken);

        if (Space is null)
            return Result.Faluire(SpaceErrors.SpaceNotFound);

        Space.IsActive = true;
        Space.RemovedAt = null;
        Space.RemovedById = null;

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<DeletedSpaceResponse>>> GetAllDeletedAsync(int WorkspaceId, string UserId, CancellationToken cancellationToken = default!)
    {
        var workspaceExists = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!workspaceExists)
            return Result.Faluire<IEnumerable<DeletedSpaceResponse>>(WorkSpaceErrors.WorkSpaceNotFound);

        var isOwner = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId , cancellationToken);

        var isMember = !isOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);

        if (!isOwner && !isMember)
            return Result.Faluire<IEnumerable<DeletedSpaceResponse>>(WorkspaceMemberErrors.AccessDenied);

        var deletedSpacesQuery = _Context.Spaces
            .Where(s => s.WorkSpaceId == WorkspaceId && s.RemovedAt != null);

        if (!isOwner)
            deletedSpacesQuery = deletedSpacesQuery.Where(s => s.IsPublic);

        var deletedSpaces = await deletedSpacesQuery
            .OrderByDescending(s => s.RemovedAt)
            .Select(s => new DeletedSpaceResponse(
                s.Id,
                s.Name,
                s.Description,
                s.IconCode,
                s.LastActivity,
                s.RemovedAt!.Value,
                s.Tasks.Count(t => t.RemovedAt == null)
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DeletedSpaceResponse>>(deletedSpaces);
    }

    public async Task<Result> MoveAsync(int WorkspaceId, string Id, string UserId, MoveSpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var sourceWorkspaceExists = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.RemovedAt == null, cancellationToken);

        if (!sourceWorkspaceExists)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

        var targetWorkspaceExists = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == request.TargetWorkspaceId && w.RemovedAt == null , cancellationToken);

        if (!targetWorkspaceExists)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);


        var canAccessSource = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken) 
            || await _Context.WorkspaceMembers
              .AnyAsync(m => m.WrokSpaceID == WorkspaceId && m.UserID == UserId, cancellationToken);



        var canAccessTarget = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == request.TargetWorkspaceId && w.CreatedById == UserId, cancellationToken)
            || await _Context.WorkspaceMembers
                .AnyAsync(m => m.WrokSpaceID == request.TargetWorkspaceId && m.UserID == UserId, cancellationToken);


        if (!canAccessSource || !canAccessTarget)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var isOwnerInSource = await _Context.WorkSpaces
            .AnyAsync(w => w.Id == WorkspaceId && w.CreatedById == UserId, cancellationToken);

        Space? space;

        if (isOwnerInSource)
        {
            space = await _Context.Spaces
                .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.RemovedAt == null, cancellationToken);
        }
        else
        {
            space = await _Context.Spaces
                .SingleOrDefaultAsync(s => s.Id == Id && s.WorkSpaceId == WorkspaceId && s.RemovedAt == null && s.IsPublic, cancellationToken);
        }

        if (space is null)
            return Result.Faluire(SpaceErrors.SpaceNotFound);

        if (space.WorkSpaceId == request.TargetWorkspaceId)
            return Result.Success();



        space.WorkSpaceId = request.TargetWorkspaceId;

        space.LastActivity = DateOnly.FromDateTime(DateTime.UtcNow);

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


}