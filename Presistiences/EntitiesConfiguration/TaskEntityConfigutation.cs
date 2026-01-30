using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class TaskEntityConfigutation : IEntityTypeConfiguration<task>
{
    public void Configure(EntityTypeBuilder<task> builder)
    {


        builder.Property(t => t.Content)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(t => t.IsTaskFinished).HasDefaultValue(false);
        builder.HasOne(x => x.Folder)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x=>x.ParentFolderId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
