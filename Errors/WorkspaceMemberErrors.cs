namespace AI_genda_API.Errors;

public class WorkspaceMemberErrors
{

    public static Error MemberNotFounded = 
        new Error("WorkSpaceMember.NotAmember","This user is not a member on this WorkSapce", StatusCodes.Status404NotFound);

    public static Error AccessDenied =
      new Error( "WorkspaceMember.AccessDenied","You don't have permission to remove members on this workspace.", StatusCodes.Status401Unauthorized);


          
         
}
