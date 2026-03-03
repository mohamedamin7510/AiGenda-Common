using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class WorkspaceUser
{   
    public int WrokSpaceID { get; set; }
    public string UserID { get; set; }


    [ForeignKey("UserID")]
    public virtual ExtendedUser User { get; set; } = new();

    [ForeignKey("UserID")]
    public virtual WorkSpace WorkSpaces { get; set; } = new();

}
