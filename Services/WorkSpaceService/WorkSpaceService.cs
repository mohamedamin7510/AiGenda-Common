
using AI_genda_API.Contracts.Workspace;
using AI_genda_API.Errors;
using System.Collections.Generic;

namespace AI_genda_API.Services.FolderService;

public class WorkSpaceService(AppContext context) : IWorkSpaceService
{
    private readonly AppContext _Context = context;

    public async Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync(CancellationToken cancellationToken = default!)
    {

        var response = await _Context.WorkSpaces
            .Where(x=>x.IsActive)
            .Select( ws => 
                    new WorkSpaceResponse(
                            ws.Id,
                            ws.Name,
                           ws.Description!,
                          ws.IconPath!,
                          ws.workspaceUsers.Where(x=> x.WrokSpaceID == ws.Id).Count(),
                          ws.Spaces
                         .SelectMany(s=> s.Tasks).Count()
                         )
             ).AsNoTracking().ToListAsync(cancellationToken);



        return Result.Success<IEnumerable<WorkSpaceResponse>?>(response);
    }

    public async Task<Result<WorkSpaceResponse>> AddAsync(WorkSpaceRequest Requset, CancellationToken cancellationToken = default!)
    {
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

    //public async Task<Result> UpdateAsync(int Id ,FolderRequset folderRequset ,  CancellationToken cancellationToken)
    //{

    //    var folder = await _Context.Folders.SingleOrDefaultAsync(x => x.Id == Id,cancellationToken);

    //    if (folder is null)
    //        return Result.Faluire(FolderErrors.FolderNotFound); 

    //   folder.Name = folderRequset.Name;

    //   folder.ParentFolderId = folderRequset.ParentFolderId;

    //   await _Context.SaveChangesAsync(cancellationToken);

    //   return Result.Success();
    //}


}
