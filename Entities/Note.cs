using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class Note
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ParentFolderId { get; set; }
    [ForeignKey("ParentFolderId")]
    public virtual Folder Folder { get; set; }
}