using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class NoteEntityConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
   
        builder.HasOne(x => x.Folder)
            .WithMany(x => x.Notes)
            .HasForeignKey(x => x.ParentFolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
