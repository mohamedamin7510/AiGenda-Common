using System.Reflection;

namespace AI_genda_API.Presistience;


public class AppContext(DbContextOptions<AppContext> dbContextOptions , IHttpContextAccessor httpContextAccessor ) : IdentityDbContext(dbContextOptions)
{
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    public DbSet<task> Tasks { get; set; }
    public DbSet<ExtendedUser> ExtendedUsers { get; set; }
    public DbSet<Folder> Folders { get; set; }


    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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
            return base.SaveChangesAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }




}
