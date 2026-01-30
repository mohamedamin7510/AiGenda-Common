using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class AuditLogging
{
    public string CreatedById { get; set; } = string.Empty!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }


    public virtual ExtendedUser CreatedBy { get; set; }
    public virtual ExtendedUser? UpdatedBy { get; set; }



}
