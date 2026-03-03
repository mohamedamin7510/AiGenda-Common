using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{

    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasOne(x => x.Space)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.SpaceId);

        builder.Property(x => x.Tittle)
            .HasMaxLength(200);

        builder.Property(x => x.Descreption)
              .HasMaxLength(400);

    }
}
