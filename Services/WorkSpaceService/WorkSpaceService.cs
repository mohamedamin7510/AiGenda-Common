using AI_genda_API.Abstractions.Const;
using AI_genda_API.Abstractions.Enums;

namespace AI_genda_API.Services.FolderService;

public class WorkSpaceService(AppContext context, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender ) 
    : IWorkSpaceService 
{
    private readonly AppContext _Context = context;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _EmailSender = emailSender;

    public async Task<Result<WorkSpaceResponse>> AddAsync(string UserId, WorkSpaceRequest Requset, CancellationToken cancellationToken = default!)
    {

        var NumofworkSpaces = _Context.WorkSpaces
             .Where(x => x.CreatedById == UserId && x.RemovedAt == null)
             .Count();

        var user = await _Context.Users.Where(x => x.Id == UserId).SingleOrDefaultAsync(cancellationToken);

        if (NumofworkSpaces >= 5 && user!.SubscriptionType == (int)UserType.Free)
            return Result.Faluire<WorkSpaceResponse>(UserErrors.SubscriptionModel);

        var WorkSpace = Requset.Adapt<WorkSpaceRequest, WorkSpace>();

        await _Context.WorkSpaces.AddAsync(WorkSpace, cancellationToken);

        await _Context.SaveChangesAsync(cancellationToken);

        var WorkSpaceMember = new WorkspaceMember
        {
            UserID = UserId,
            WrokSpaceID = WorkSpace.Id,
            IsOwner = true,
            JoinedAt = DateTime.UtcNow,
            Permissions = OwnerPermissions.GetAllPerimision
        };

        await _Context.WorkspaceMembers.AddAsync(WorkSpaceMember, cancellationToken);

        await _Context.SaveChangesAsync(cancellationToken);

        var response = WorkSpace.Adapt<WorkSpace, WorkSpaceResponse>();

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var UserId = _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var response = await _Context.WorkSpaces
            .Where(ws =>
                ws.RemovedAt == null &&
                (
                    ws.CreatedById == UserId ||
                    ws.workspaceMembers.Any(m => m.UserID == UserId)
                ))
            .Select(ws => new WorkSpaceResponse(
                ws.Id,
                ws.Name,
                ws.Description!,
                ws.IconCode!,
                ws.Visibility,
                ws.workspaceMembers.Count(m => m.WrokSpaceID == ws.Id),
                ws.Spaces
                    .Where(s => s.IsActive && s.RemovedAt == null)
                    .SelectMany(s => s.Tasks.Where(t => t.IsActive && t.RemovedAt == null))
                    .Count(),
                ws.CreatedById == UserId
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<WorkSpaceResponse>?>(response);
    }

    public async Task<Result<WorkSpaceResponse>> GetByIdAsync(int id, string? userId, CancellationToken cancelationToken)
    {


        var response = await _Context.WorkSpaces
            .Where(ws =>
                ws.Id == id &&
                ws.RemovedAt == null &&
                (
                    ws.CreatedById == userId ||
                    ws.workspaceMembers.Any(m => m.UserID == userId && m.WrokSpaceID == id)
                ))
            .Select(ws => new WorkSpaceResponse(
                ws.Id,
                ws.Name,
                ws.Description!,
                ws.IconCode!,
                ws.Visibility,
                ws.workspaceMembers.Count(m => m.WrokSpaceID == ws.Id),
                ws.Spaces
                    .Where(s => s.IsActive && s.RemovedAt == null)
                    .SelectMany(s => s.Tasks.Where(t => t.IsActive && t.RemovedAt == null))
                    .Count(),
                ws.CreatedById == userId
            ))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancelationToken);

        if (response is null)
            return Result.Faluire<WorkSpaceResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        return Result.Success(response);
    }

    public async Task<Result<WorkspaceDashboardResponse>> GetWorkspaceDashboardAsync(int Id,string UserId, CancellationToken cancellationToken = default) 
           
    {
        // Validate workspace existence

        // Validate userId access to workspace

        // validate user that request the dashboard is the member of the workspace or not or who created it (two status)

        // Fetch statistics

        // Fetch weekly focus data

        // Fetch recent activities

        // Fetch priority tasks

        // Fetch all spaces in the workspace 

        // Map to WorkspaceDashboardResponse

        return Result.Success(new WorkspaceDashboardResponse(
        new DashboardStatsResponse(0,0,0,0), // Replace with actual stats
        new WeeklyFocusTimeResponse(new List<FocusDayResponse>()), // Replace with actual focus time data
        new List<ActivityResponse>(), // Replace with actual recent activities
        new List<PriorityTaskResponse>(), // Replace with actual priority tasks
        new List<SpaceResponse>() // Replace with actual spaces

       ));
    }

    public async Task<Result<WorkSpaceResponse>> UpdateAsync(int Id,  string UserId ,  WorkSpaceRequest requset, CancellationToken cancellationToken)
    {

        var workSpace = await _Context.WorkSpaces.
            Where(x => x.CreatedById == UserId && x.Id == Id && x.RemovedAt == null)
            .SingleOrDefaultAsync(cancellationToken);

        if (workSpace is null)
            return Result.Faluire<WorkSpaceResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        workSpace.Name = requset.Name;
        workSpace.Description = requset.Description;
        workSpace.IconCode = requset.IconCode;
        workSpace.Visibility = requset.Visibility;

        await _Context.SaveChangesAsync(cancellationToken);

        var response = workSpace.Adapt<WorkSpace, WorkSpaceResponse>();

        return Result.Success(response);
    }
   
    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {

        var workspace = await _Context.WorkSpaces.SingleOrDefaultAsync(x => x.Id == id && x.RemovedAt == null, cancellationToken);

        if (workspace is  null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

           workspace.IsActive = false; 
           workspace.RemovedAt = DateTime.UtcNow;
           workspace.RemovedById = _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

           await _Context.SaveChangesAsync(cancellationToken);

          return   Result.Success();
    }

    public async Task<Result> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var workspace = await _Context.WorkSpaces.SingleOrDefaultAsync(x => x.Id == id && x.RemovedAt != null, cancellationToken);

        if (workspace is null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

        workspace.IsActive = true;
        workspace.RemovedAt = null;
        workspace.RemovedById = null;
    
        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AddMemberAsync ( int Id ,string UserId, InviteMember request, CancellationToken cancellationToken)
    {
        var workspace = await _Context.WorkSpaces
            .Where(x => x.CreatedById == UserId && x.Id == Id && x.RemovedAt == null)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        
        if (workspace is null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

        if (workspace.Visibility == 0)
            return Result.Faluire(WorkspaceMemberErrors.InvalidAssign);

        var normalizedEmail = request.email.Trim().ToUpper();

        var invitedUser = await _Context.Users
            .Where(x => x.NormalizedEmail == normalizedEmail && !x.IsDisabled)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (invitedUser is null)
            return Result.Faluire(UserErrors.EmailnotFounded);

        var isExistsBefore = await _Context.WorkspaceMembers
            .AnyAsync(x => x.UserID == invitedUser.Id && x.WrokSpaceID == Id, cancellationToken);

        if (isExistsBefore)
            return Result.Faluire(UserErrors.AddedMemberBefore);

        var inviterUser = await _Context.Users
            .Where(x => x.Id == UserId && !x.IsDisabled)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (inviterUser is null)
            return Result.Faluire(UserErrors.UserNotFounded);

        var workSpaceMember = new WorkspaceMember
        {
            UserID = invitedUser.Id,
            WrokSpaceID = Id,
            IsOwner = false,
            JoinedAt = DateTime.UtcNow,
            Permissions =
            [
               Permissions.GetWorkSpaces, 
               Permissions.GetSpaces,
               Permissions.GetTasks,
               Permissions.GetNotes
            ]
        };

        await _Context.WorkspaceMembers.AddAsync(workSpaceMember, cancellationToken);

        await _Context.SaveChangesAsync(cancellationToken);

        await SendEmailInvite(inviterUser, invitedUser, workspace, "✅ AiGenda Team: Addition Service");

        return Result.Success();


      }
    
    public async Task<Result> RemoveMemberAsync(int Id, string RemoverUserId, InviteMember request, CancellationToken cancellationToken = default!)
    {

        var workspace = await _Context.WorkSpaces
          .Where(w => w.Id == Id && w.RemovedAt == null)
          .Select(w => new { w.Id, w.CreatedById })
          .SingleOrDefaultAsync(cancellationToken);

        if (workspace is null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

        var normalizedEmail = request.email.Trim().ToUpperInvariant();

        var removedMember = await _Context.Users
            .Where(x => x.NormalizedEmail == normalizedEmail)
            .Select(x => new { x.Id })
            .SingleOrDefaultAsync(cancellationToken);

        if (removedMember is null)
            return Result.Faluire(UserErrors.EmailnotFounded);

        var isOwner = workspace.CreatedById == RemoverUserId;
        var isSelfRemoval = removedMember.Id == RemoverUserId;

        if (!isOwner && !isSelfRemoval)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        // Safety: do not allow removing workspace creator membership record
        if (removedMember.Id == workspace.CreatedById)
            return Result.Faluire(WorkspaceMemberErrors.AccessDenied);

        var memberRecord = await _Context.WorkspaceMembers
            .SingleOrDefaultAsync(m => m.WrokSpaceID == Id && m.UserID == removedMember.Id, cancellationToken);

        if (memberRecord is null)
            return Result.Faluire(WorkspaceMemberErrors.MemberNotFounded);

        _Context.WorkspaceMembers.Remove(memberRecord);

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();

        
    }

    public async Task<Result<IEnumerable<WorkspaceMemberResponse>>> GetMembersAsync(int Id, string UserId, CancellationToken cancellationToken = default!)
    {
        var workspace = await _Context.WorkSpaces
            .Where(w => w.Id == Id && w.RemovedAt == null)
            .Select(w => new { w.Id, w.CreatedById })
            .SingleOrDefaultAsync(cancellationToken);

        if (workspace is null)
            return Result.Faluire<IEnumerable<WorkspaceMemberResponse>>(WorkSpaceErrors.WorkSpaceNotFound);

        var isOwner = workspace.CreatedById == UserId;

        var isMember = !isOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == Id && m.UserID == UserId, cancellationToken);

        if (!isOwner && !isMember)
            return Result.Faluire<IEnumerable<WorkspaceMemberResponse>>(WorkspaceMemberErrors.AccessDenied);

        var members = await _Context.WorkspaceMembers
            .Where(m => m.WrokSpaceID == Id)
            .OrderByDescending(m => m.IsOwner)
            .ThenBy(m => m.JoinedAt)
            .Select(m => new WorkspaceMemberResponse(
                m.UserID,
                (m.User!.FirstName + " " + m.User.SecondName).Trim(),
                m.User.Email ?? string.Empty,
                m.IsOwner,
                m.JoinedAt,
                m.Permissions
            ))
            .AsNoTracking()        
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<WorkspaceMemberResponse>>(members);
    }

    public async Task<Result<WorkspaceMemberResponse>> UpdateMemberPermissionsAsync(int Id,string OwnerUserId,string MemberUserId,UpdateWorkspaceMemberPermissionsRequest request,CancellationToken cancellationToken = default!)         
  {
        var workspace = await _Context.WorkSpaces
            .Where(w => w.Id == Id && w.RemovedAt == null)
            .Select(w => new { w.Id, w.CreatedById })
            .SingleOrDefaultAsync(cancellationToken);

        if (workspace is null)
            return Result.Faluire<WorkspaceMemberResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        // Only workspace owner can update member permissions
        if (workspace.CreatedById != OwnerUserId)
            return Result.Faluire<WorkspaceMemberResponse>(WorkspaceMemberErrors.AccessDenied);

        var memberRecord = await _Context.WorkspaceMembers
            .SingleOrDefaultAsync(m => m.WrokSpaceID == Id && m.UserID == MemberUserId, cancellationToken);

        if (memberRecord is null)
            return Result.Faluire<WorkspaceMemberResponse>(WorkspaceMemberErrors.MemberNotFounded);

        // Do not allow editing owner membership permissions
        if (memberRecord.IsOwner || MemberUserId == workspace.CreatedById)
            return Result.Faluire<WorkspaceMemberResponse>(WorkspaceMemberErrors.OwnerPermissionsImmutable);

        var allowedPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
         
            Permissions.UpdateSpaces,
            Permissions.AddSpaces,
            Permissions.DeleteSpaces,
          
            Permissions.UpdateTasks,
            Permissions.AddTasks,
            Permissions.DeleteTasks,
       
            Permissions.UpdateNotes,
            Permissions.AddNotes,
            Permissions.DeleteNotes
        };

        var requested = request.Permissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var hasInvalidPermission = requested.Any(p => !allowedPermissions.Contains(p));
        if (hasInvalidPermission)
            return Result.Faluire<WorkspaceMemberResponse>(WorkspaceMemberErrors.InvalidPermissions);

        // Baseline workspace visibility must stay enabled for any member
        if (!requested.Contains(Permissions.GetWorkSpaces, StringComparer.OrdinalIgnoreCase))
        {
            requested.Insert(0, Permissions.GetWorkSpaces);
            requested.Insert(1, Permissions.GetSpaces);
            requested.Insert(2, Permissions.GetTasks);
            requested.Insert(3, Permissions.GetNotes);

        }

        memberRecord.Permissions = requested;

        await _Context.SaveChangesAsync(cancellationToken);

        var response = await _Context.WorkspaceMembers
            .Where(m => m.WrokSpaceID == Id && m.UserID == MemberUserId)
            .Select(m => new WorkspaceMemberResponse(
                m.UserID,
                (m.User!.FirstName + " " + m.User.SecondName).Trim(),
                m.User.Email ?? string.Empty,
                m.IsOwner,
                m.JoinedAt,
                m.Permissions
            ))
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        return Result.Success(response);
}
   
   public async Task<Result<WorkspaceMemberPermissionsResponse>> GetMemberPermissionsAsync(int Id,string RequesterUserId,string MemberUserId,CancellationToken cancellationToken = default!)   
   {
        var workspace = await _Context.WorkSpaces
            .Where(w => w.Id == Id && w.RemovedAt == null)
            .Select(w => new { w.Id, w.CreatedById })
            .SingleOrDefaultAsync(cancellationToken);

        if (workspace is null)
            return Result.Faluire<WorkspaceMemberPermissionsResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var isOwner = workspace.CreatedById == RequesterUserId;

        var requesterIsMember = !isOwner && await _Context.WorkspaceMembers
            .AnyAsync(m => m.WrokSpaceID == Id && m.UserID == RequesterUserId, cancellationToken);

      
        if (!isOwner && !requesterIsMember)
            return Result.Faluire<WorkspaceMemberPermissionsResponse>(WorkspaceMemberErrors.AccessDenied);

        
        if (!isOwner && RequesterUserId != MemberUserId)
            return Result.Faluire<WorkspaceMemberPermissionsResponse>(WorkspaceMemberErrors.AccessDenied);

        if (MemberUserId == workspace.CreatedById)
        {
            return Result.Success(new WorkspaceMemberPermissionsResponse(
                MemberUserId,
                true,
                OwnerPermissions.GetAllPerimision
            ));
        }

        var memberPermissions = await _Context.WorkspaceMembers
            .Where(m => m.WrokSpaceID == Id && m.UserID == MemberUserId)
            .Select(m => new WorkspaceMemberPermissionsResponse(
                m.UserID,
                m.IsOwner,
                m.Permissions
            ))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (memberPermissions is null)
            return Result.Faluire<WorkspaceMemberPermissionsResponse>(WorkspaceMemberErrors.MemberNotFounded);

        return Result.Success(memberPermissions);
   }

    public async Task<Result<IEnumerable<DeletedWorkSpaceResponse>>> GetAllDeletedAsync(string UserId, CancellationToken cancellationToken = default!)
    {
        var response = await _Context.WorkSpaces
            .Where(ws => ws.CreatedById == UserId && ws.RemovedAt != null)
            .OrderByDescending(ws => ws.RemovedAt)
            .Select(ws => new DeletedWorkSpaceResponse(
                ws.Id,
                ws.Name,
                ws.Description!,
                ws.IconCode!,
                ws.Visibility,
                ws.RemovedAt!.Value,
                ws.workspaceMembers.Count(),
                ws.Spaces
                    .Where(s => s.IsActive && s.RemovedAt == null)
                    .SelectMany(s => s.Tasks.Where(t => t.IsActive && t.RemovedAt == null))
                    .Count()
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DeletedWorkSpaceResponse>>(response);
    }

    private async  System.Threading.Tasks.Task SendEmailInvite( ExtendedUser User , ExtendedUser InvitedUser , WorkSpace Workspace , string Message)
    {
        var origin = _HttpContextAccessor.HttpContext?.Request.Headers.Origin;

        var BuilderMessage = EmailBodyBuilder.GenerateEmailBody("WorkspaceMemberInvite",

        new Dictionary<string, string>
            {
                        {"{{WorkSpaceName}}", Workspace.Name},
                        {"{{FullName}}", InvitedUser.FirstName + " "+ InvitedUser.SecondName },
                        {"{{AddedUserName}}", User.FirstName + " "+ User.SecondName },
                        { "{{action_url}}", $"{origin}/WorkSpace/Add-member?workspaceId={Workspace.Id}&&UserId={User.Id}" } 
            }
        );

      BackgroundJob.Enqueue(()=> _EmailSender.SendEmailAsync(User.Email!, Message, BuilderMessage));

    }

  
}
