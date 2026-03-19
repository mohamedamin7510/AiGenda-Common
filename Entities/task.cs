using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class Task : AuditLogging
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Tittle { get; set; }
    public string? Descreption { get; set; } = default!;

    public bool IsActive { get; set; } = true;
    public string Status { get; set; }
    public string SpaceId { get; set; }

    [ForeignKey("SpaceId")]
    public Space Space { get; set; }

    // todo: Complete the resident columns 
    // todo: Assignee users one-to-many relationship  with the user
}
