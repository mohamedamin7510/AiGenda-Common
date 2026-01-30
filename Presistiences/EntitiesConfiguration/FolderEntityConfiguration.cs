using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class FolderEntityConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(50).HasDefaultValue(" ");

        builder.HasOne(x => x.ParentFolder)
            .WithMany(x => x.ChildFolders)
            .HasForeignKey(x => x.ParentFolderId);

           


    }
}
