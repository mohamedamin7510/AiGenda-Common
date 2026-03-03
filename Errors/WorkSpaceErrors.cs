namespace AI_genda_API.Errors;

public class WorkSpaceErrors
{
    public static Error WorkSpaceNotFound => new Error("WorkSpace.NotFound", "This WorkSpace is not existed", StatusCodes.Status404NotFound); 
}
