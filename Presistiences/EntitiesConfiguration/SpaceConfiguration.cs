using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{

    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.HasKey(x => x.Id) ;

        builder.HasOne(x => x.WorkSpace)
            .WithMany(x => x.Spaces)
            .HasForeignKey(x => x.WorkSpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Name)
            .HasMaxLength(60);

        builder.Property(x => x.Descreption)
              .HasMaxLength(300);

        builder.Property(x => x.IconHexa)
              .HasMaxLength(500);


    }
}
