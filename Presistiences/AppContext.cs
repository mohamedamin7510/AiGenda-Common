using System.Reflection;
using System.Security.Claims;
using AI_genda_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = AI_genda_API.Entities.Task;
using AI_genda_API.Presistiences.EntitiesConfiguration;
using AI_genda_API.Services.TokenManagement; // تم تغيير الـ namespace الخاص بالتشفير هنا

namespace AI_genda_API.Presistience;

public class AppContext : IdentityDbContext<ExtendedUser, ApplicationRole, string>
{
    private readonly IHttpContextAccessor _HttpContextAccessor;
    private readonly ITokenEncryptionService? _tokenEncryptionService; // تم التعديل هنا

    public AppContext(DbContextOptions<AppContext> dbContextOptions, IHttpContextAccessor httpContextAccessor, ITokenEncryptionService? tokenEncryptionService = null)
        : base(dbContextOptions)
    {
        _HttpContextAccessor = httpContextAccessor;
        _tokenEncryptionService = tokenEncryptionService; // تم التعديل هنا
    }
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

        // تم ربط الـ Configuration بالـ Service الجديدة المتوافقة مع التيم
        modelBuilder.ApplyConfiguration(new AppConnectionConfiguration(_tokenEncryptionService));

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