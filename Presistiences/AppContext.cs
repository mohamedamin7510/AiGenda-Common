using Task = AI_genda_API.Entities.Task;
namespace AI_genda_API.Presistience;


public class AppContext(DbContextOptions<AppContext> dbContextOptions , IHttpContextAccessor httpContextAccessor ) : IdentityDbContext(dbContextOptions)
{
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    public DbSet<ExtendedUser> Users { get; set; }
    public DbSet<WorkSpace> WorkSpaces { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<Space> Spaces{ get; set; }
    public DbSet<Task> Tasks{ get; set; }



    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ClaimId = _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Entries = ChangeTracker.Entries<AuditLogging>();
        foreach (var entry in Entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedById).CurrentValue = ClaimId!;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.UpdatedById).CurrentValue = ClaimId;
                entry.Property(x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }

        }
            return base.SaveChangesAsync(cancellationToken);
    }
    public override int SaveChanges()
    {
        var ClaimId = _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var Entries = ChangeTracker.Entries<AuditLogging>();
        foreach (var entry in Entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedById).CurrentValue = ClaimId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.UpdatedById).CurrentValue = ClaimId;
                entry.Property(x => x.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }

        }
        return base.SaveChanges();

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


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
