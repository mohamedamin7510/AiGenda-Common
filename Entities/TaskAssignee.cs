using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

[Table("TaskAssignees")]
public class TaskAssignee
{
    public string TaskId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }

    [ForeignKey("TaskId")]
    public virtual SpaceTask? Task { get; set; }

    [ForeignKey("UserId")]
    public virtual ExtendedUser? User { get; set; }
}