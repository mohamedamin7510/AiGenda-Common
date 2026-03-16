using Hangfire;
using Microsoft.Data.SqlClient;

namespace AI_genda_API.Services.FolderService;

public class WorkSpaceService(AppContext context, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender ) 
    : IWorkSpaceService 
{
    private readonly AppContext _Context = context;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _EmailSender = emailSender;

    public async Task<Result<WorkSpaceResponse>> AddAsync(string UserId, WorkSpaceRequest Requset, CancellationToken cancellationToken = default!)
    {
        var numofworkSpaces = _Context.WorkSpaces
            .Where(x => x.CreatedById == UserId && x.RemovedAt == null ) 
            .Count();

        var user = await _Context.Users.Where(x => x.Id == UserId).SingleOrDefaultAsync(cancellationToken); 


        if (numofworkSpaces > 5 && user!.SubscriptionType == "Free")
               return Result.Faluire<WorkSpaceResponse>(UserErrors.SubscriptionModel);
        

        var WorkSpace = Requset.Adapt<WorkSpaceRequest, WorkSpace>();

        await _Context.WorkSpaces.AddAsync(WorkSpace, cancellationToken);

        await _Context.SaveChangesAsync();

        var response = WorkSpace.Adapt<WorkSpace, WorkSpaceResponse>();

        return  Result.Success(response);
    }

    public async Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var UserId = _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!.ToString();

        var response = await _Context.WorkSpaces
            .Where(x=>x.RemovedAt == null && x.CreatedById  == UserId)
            .Select( ws => 
                    new WorkSpaceResponse(
                            ws.Id,
                            ws.Name,
                           ws.Description!,
                          ws.IconCode!,
                          ws.Visibility,
                          ws.workspaceMembers.Where(x=> x.WrokSpaceID == ws.Id).Count(),
                          ws.Spaces.SelectMany(s=> s.Tasks).Count()                        
                         )
             ).AsNoTracking().ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<WorkSpaceResponse>?>(response);
    }

    public async Task<Result<WorkSpaceResponse>> GetByIdAsync(int id, string? userId, CancellationToken cancelationToken)
    {

        var workSpace = await _Context.WorkSpaces.
            Where(x => x.CreatedById == userId && x.Id == id && x.RemovedAt == null)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancelationToken);

        if (workSpace is null)
            return Result.Faluire<WorkSpaceResponse>(WorkSpaceErrors.WorkSpaceNotFound);

        var response = workSpace.Adapt<WorkSpace, WorkSpaceResponse>();

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

        var workspace = await _Context.WorkSpaces.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (workspace is  null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

           workspace.IsActive = false; 
           workspace.RemovedAt = DateTime.UtcNow;
           workspace.RemovedById = _HttpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.ToString();

           await _Context.SaveChangesAsync(cancellationToken);

          return   Result.Success();
    }

    public async Task<Result> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var workspace = await _Context.WorkSpaces.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

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

     
        var Workspace = await _Context.WorkSpaces.Where(x => x.CreatedById == UserId && x.Id == Id)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (Workspace is null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

     
        var InvitedUser = await _Context.Users.Where(x => x.Email == request.email)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (InvitedUser is null)
            return Result.Faluire(UserErrors.EmailnotFounded);
     
        var ISExisExistBefore = _Context.WorkspaceMembers.Any(x => x.UserID == InvitedUser.Id && x.WrokSpaceID == Id);

        if (ISExisExistBefore)
            return Result.Faluire(UserErrors.AddedMemberBefore);

        var User = await _Context.Users.Where(x => x.Id == UserId)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        var WorkSpaceMember = new WorkspaceMember
        {
            UserID = InvitedUser.Id,
            WrokSpaceID = Id,
            JoinedAt = DateTime.UtcNow
        };

        await _Context.WorkspaceMembers.AddAsync(WorkSpaceMember);

        await _Context.SaveChangesAsync(cancellationToken);


        await SendEmailInvite(User!, InvitedUser , Workspace, "️✅ AiGenda Team: Addtion Service");

        return Result.Success();
    }
    
    public async Task<Result> RemoveMemberAsync(int Id, string RemoverUserId, InviteMember request, CancellationToken cancellationToken = default!)
    {


        var workspaceExists = await _Context.WorkSpaces.AnyAsync(w => w.Id == Id && w.RemovedAt == null, cancellationToken);
           
        if (!workspaceExists)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);


        var RemovedMember = await _Context.Users.Where(x => x.Email == request.email)
            .SingleOrDefaultAsync(cancellationToken);

        if (RemovedMember is null)
            return Result.Faluire(UserErrors.EmailnotFounded);

        if ((RemoverUserId != RemovedMember.Id))
        {

            var IsAuthorized = await _Context.WorkSpaces.AnyAsync(w => w.CreatedById == RemoverUserId && w.Id == Id, cancellationToken);

            if (!IsAuthorized)
                return Result.Faluire(WorkspaceMemberErrors.AccessDenied);
        }


            var IsMember = await _Context.WorkspaceMembers
               .AnyAsync(m => m.WrokSpaceID == Id && m.UserID == RemovedMember.Id, cancellationToken);

            if (!IsMember)
                return Result.Faluire(WorkspaceMemberErrors.MemberNotFounded);


            var Removedrecord = await _Context.WorkspaceMembers
                .Where(x => x.WrokSpaceID == Id && x.UserID == RemovedMember.Id)
                .SingleOrDefaultAsync(cancellationToken);

                _Context.WorkspaceMembers.Remove(Removedrecord!);
        
                await _Context.SaveChangesAsync(cancellationToken);


            return Result.Success();
        


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
                        { "{{action_url}}", $"https://{origin}/WorkSpace/Add-member?workspaceId={Workspace.Id}&&UserId={User.Id}" } 
            }
        );

      BackgroundJob.Enqueue(()=> _EmailSender.SendEmailAsync(User.Email!, Message, BuilderMessage));

    }
}
