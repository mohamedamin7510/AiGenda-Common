using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class Folder: AuditLogging
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    //self refrence
    public int? ParentFolderId { get; set; }
    public virtual Folder? ParentFolder { get; set; }

    // Folders
    public virtual List<Folder> ChildFolders { get; set; } = new ();

    //Tasks
    public virtual List<task> Tasks { get; set; } = new();
    //Notes
    public virtual List<Note> Notes { get; set; } = new();


}
