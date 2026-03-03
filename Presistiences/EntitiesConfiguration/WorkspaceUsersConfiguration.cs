using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class WorkspaceUsersConfiguration : IEntityTypeConfiguration<WorkspaceUser>
{
    public void Configure(EntityTypeBuilder<WorkspaceUser> builder)
    {
        builder.HasKey(x => new {x.WrokSpaceID , x.UserID });

        builder.HasOne(x => x.WorkSpaces)
            .WithMany(x=>x.workspaceUsers);
    }
}
