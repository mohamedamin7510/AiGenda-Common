using System.Reflection;
using System.Security.Claims;
using AI_genda_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = AI_genda_API.Entities.Task;

using AI_genda_API.Presistiences.EntitiesConfiguration;
using Microsoft.AspNetCore.DataProtection;

namespace AI_genda_API.Presistience;

public class AppContext : IdentityDbContext<ExtendedUser, ApplicationRole, string>
{
    private readonly IHttpContextAccessor _HttpContextAccessor;
    private readonly IDataProtectionProvider? _dataProtectionProvider;

    public AppContext(DbContextOptions<AppContext> dbContextOptions, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider? dataProtectionProvider = null) 
        : base(dbContextOptions)
    {
        _HttpContextAccessor = httpContextAccessor;
        _dataProtectionProvider = dataProtectionProvider;
    }

    public DbSet<ExtendedUser> Users { get; set; }
    public DbSet<WorkSpace> WorkSpaces { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<Space> Spaces { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskAssignee> TaskAssignees { get; set; }
    public DbSet<SubTask> SubTasks { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<NoteAsset> NoteAssets { get; set; }
    public DbSet<TextNoteContent> TextNoteContents { get; set; }
    public DbSet<VoiceNoteContent> VoiceNoteContents { get; set; }
    public DbSet<ImageNoteContent> ImageNoteContents { get; set; }
    public DbSet<HandDrawNoteContent> HandDrawNoteContents { get; set; }
    public DbSet<FocusSession> FocusSessions { get; set; }
    public DbSet<AppConnection> AppConnections { get; set; }
    public DbSet<LinkedData> LinkedData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            // Do not force SqlServer here to avoid provider conflict
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ClaimId = _HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Entries = ChangeTracker.Entries<AuditLogging>();
        foreach (var entry in Entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrEmpty(entry.Entity.CreatedById) && ClaimId != null)
                {
                    entry.Property(x => x.CreatedById).CurrentValue = ClaimId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (ClaimId != null)
                {
                    entry.Property(x => x.UpdatedById).CurrentValue = ClaimId;
                }
                entry.Property(x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        var ClaimId = _HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Entries = ChangeTracker.Entries<AuditLogging>();
        foreach (var entry in Entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrEmpty(entry.Entity.CreatedById) && ClaimId != null)
                {
                    entry.Property(x => x.CreatedById).CurrentValue = ClaimId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (ClaimId != null)
                {
                    entry.Property(x => x.UpdatedById).CurrentValue = ClaimId;
                }
                entry.Property(x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Custom Configuration Injection with DP
        modelBuilder.ApplyConfiguration(new AppConnectionConfiguration(_dataProtectionProvider));

        var CascadeFks = modelBuilder.Model
            .GetEntityTypes().SelectMany(f => f.GetForeignKeys())
            .Where(x => x.DeleteBehavior == DeleteBehavior.Cascade && !x.IsOwnership);

        foreach (var fk in CascadeFks)
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        base.OnModelCreating(modelBuilder);
    }
}