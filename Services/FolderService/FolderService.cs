
using AI_genda_API.Contracts.Folders;

namespace AI_genda_API.Services.FolderService;

public class FolderService(AppContext context) : IFolderService
{
    private readonly AppContext _Context = context;

 
    public async Task<List<FolderResponse>?> GetAllFoldersAsync(CancellationToken cancellationToken = default!)
    {
        var Folders = await  _Context.Folders.AsNoTracking().ToListAsync(cancellationToken);
        return Folders.Adapt<List<Folder>, List<FolderResponse>>();
    }

    public async Task<FolderResponse> AddFolderAsync(FolderRequset folderRequset , CancellationToken cancellationToken = default! )
    {
        var folder = folderRequset.Adapt<FolderRequset , Folder>();
        await _Context.Folders.AddAsync( folder ,cancellationToken );
        await  _Context.SaveChangesAsync(cancellationToken);
        return folder.Adapt<Folder,FolderResponse>();
    }

    public async Task<bool> DeleteFolderAsync(int id, CancellationToken cancellationToken)
    {

        //todo:Get the target folder and load for it the child flders then foreach on them then remove it then remove the targer folder .
        var folder = await _Context.Folders.SingleOrDefaultAsync(x => x.Id == id,cancellationToken);
        if (folder is null)
            return false; 
       _Context.Folders.Remove(folder);
       await  _Context.SaveChangesAsync(cancellationToken);

        return true;
    }
    public async Task<bool> UpdateFolderAsync(int Id ,FolderRequset folderRequset ,  CancellationToken cancellationToken)
    {

        var folder = await _Context.Folders.SingleOrDefaultAsync(x => x.Id == Id,cancellationToken);
        if (folder is null)
            return false; 
      
        folder.Name = folderRequset.Name;
        folder.ParentFolderId = folderRequset.ParentFolderId;
       await  _Context.SaveChangesAsync(cancellationToken);

        return true;
    }


}
