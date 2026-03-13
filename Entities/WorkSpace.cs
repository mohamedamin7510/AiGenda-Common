namespace AI_genda_API.Entities;

public class WorkSpace:AuditLogging
{
    public int Id { get; set; }
    public  string Name { get; set; }
    public  string?  Description { get; set; }
    public string? IconCode { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual List<WorkspaceMember> workspaceUsers { get; set; }
    public virtual List<Space> Spaces { get; set; } = new();


}
