using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class Space : AuditLogging
{
    public string Id { get; set; }= Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string? Descreption { get; set; }
    public string IconHexa { get; set; }
    public DateOnly LastActivity  { get; set; }
    public int WorkSpaceId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true;

   
    public virtual WorkSpace WorkSpace { get; set; }
    public virtual List<Task> Tasks { get; set; } = new List<Task>();


}
