using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class TaskConfiguration : IEntityTypeConfiguration<SpaceTask>
{
    public void Configure(EntityTypeBuilder<SpaceTask> builder)
    {
        builder.HasOne(x => x.Space)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.SpaceId);

        builder.Property(x => x.Title)
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(400);
    }
}