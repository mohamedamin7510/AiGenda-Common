using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

[Table("WorkspaceMembers")]
public class WorkspaceMember
{   
    public int WrokSpaceID { get; set; }
    public string UserID { get; set; }
    public DateTime JoinedAt { get; set; }


    [ForeignKey("UserID")]
    public virtual ExtendedUser? User { get; set; }

    [ForeignKey("WrokSpaceID")]
    public virtual WorkSpace? WorkSpaces { get; set; }

}
