
using AI_genda_API.Contracts.Workspace;
using AI_genda_API.Errors;
using BucketSurvey.Api.Helpers;
using Hangfire;
using System.Collections.Generic;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace AI_genda_API.Services.FolderService;

public class WorkSpaceService(AppContext context, 
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender
    
    ) : IWorkSpaceService
{
    private readonly AppContext _Context = context;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _EmailSender = emailSender;

    public async Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync(CancellationToken cancellationToken = default!)
    {

        var response = await _Context.WorkSpaces
            .Where(x=>x.IsActive)
            .Select( ws => 
                    new WorkSpaceResponse(
                            ws.Id,
                            ws.Name,
                           ws.Description!,
                          ws.IconCode!,
                          ws.workspaceUsers.Where(x=> x.WrokSpaceID == ws.Id).Count(),
                          ws.Spaces
                         .SelectMany(s=> s.Tasks).Count()
                         )
             ).AsNoTracking().ToListAsync(cancellationToken);



        return Result.Success<IEnumerable<WorkSpaceResponse>?>(response);
    }

    public async Task<Result<WorkSpaceResponse>> AddAsync(WorkSpaceRequest Requset, CancellationToken cancellationToken = default!)
    {

        var UserId = _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);// More Secured Way
      
        var user = _Context.Users.SingleOrDefault(x => x.Id == UserId);

        if (_Context.WorkSpaces.Count() >= 5 &&  user!.SubscriptionType == "Free")
        {
             return Result.Faluire<WorkSpaceResponse>(UserErrors.SubscriptionModel);
        }

        var WorkSpace = Requset.Adapt<WorkSpaceRequest, WorkSpace>();

        await _Context.WorkSpaces.AddAsync(WorkSpace, cancellationToken);

        await _Context.SaveChangesAsync();

        var response = WorkSpace.Adapt<WorkSpace, WorkSpaceResponse>();

        return Result.Success(response);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {

        if (await _Context.WorkSpaces.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) is { } WorkSpace )
        {

            WorkSpace.IsActive = false; 

            await _Context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);
    }
   
    public async Task<Result> UpdateAsync(string UserId , int Id, WorkSpaceRequest requset, CancellationToken cancellationToken)
    {

        var workSpace = await _Context.Users.
            Where(x => x.Id == UserId && x.WorkSpaceUser .WrokSpaceID == Id)
            .Select(x => x.WorkSpaceUser.WorkSpaces)
            .SingleOrDefaultAsync(cancellationToken);

        if (workSpace is null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

        workSpace.Name = requset.Name;
        workSpace.Description = requset.Description;
        workSpace.IconCode = requset.IconCode;

        await _Context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AddMemberAsync ( int WorkSpaceId ,string UserId, InviteMemberRequest request, CancellationToken cancellationToken)
    {

     
        // if the iviter is existed in this workspace or not
        var Workspace = await  _Context.WorkSpaces.Where(x => x.Id == WorkSpaceId && x.CreatedById  == UserId ).SingleOrDefaultAsync();

        if (Workspace is null)
            return Result.Faluire(WorkSpaceErrors.WorkSpaceNotFound);

        // Get The MemberId 
        var InvitedUser = await _Context.Users.Where(x => x.Email == request.email).SingleOrDefaultAsync();

        if (InvitedUser is null)
            return Result.Faluire(UserErrors.EmailnotFounded);

        // check if the user is already a member of the workspace
        var ISExisExistBefore = _Context.WorkspaceMembers.Any(x => x.UserID == InvitedUser.Id && WorkSpaceId == x.WrokSpaceID);

        if (ISExisExistBefore)
            return Result.Faluire(UserErrors.AddedMemberBefore);

        var User = await _Context.Users.Where(x => x.Id == UserId).SingleOrDefaultAsync();


        await _Context.Database.ExecuteSqlRawAsync(
        "INSERT INTO WorkspaceMembers (UserID, WrokSpaceID, JoinedAt) VALUES ({0}, {1} , {2})",
        InvitedUser.Id, WorkSpaceId,DateTime.UtcNow
       );


        var origin = _HttpContextAccessor.HttpContext?.Request.Headers.Origin;

        var BuilderMessage = EmailBodyBuilder.GenerateEmailBody("WorkspaceMemberInvite",

        new Dictionary<string, string>
            {
                        {"{{WorkSpaceName}}", Workspace.Name},
                        {"{{FullName}}", InvitedUser.FirstName + " "+ InvitedUser.SecondName },
                        {"{{AddedUserName}}", User.FirstName + " "+ User.SecondName },
                        { "{{action_url}}", $"{origin}/Authentication/Add-member?" } //todo: redirect to this workspace
            }
        );

        BackgroundJob.Enqueue(
            () => _EmailSender.SendEmailAsync(User.Email!, "️✅ AiGenda Team: Addtion Service", BuilderMessage));





        return Result.Success();
    }
}
