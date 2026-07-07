using AI_genda_API.Entities;
using AI_genda_API.Services.TokenManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class AppConnectionConfiguration : IEntityTypeConfiguration<AppConnection>
{
    private readonly ITokenEncryptionService? _tokenEncryptionService;

    // الـ Parameterless Constructor هيفضل موجود عشان الـ Migration Scanning ميعملش مشاكل
    public AppConnectionConfiguration()
    {
        _tokenEncryptionService = null;
    }

    public AppConnectionConfiguration(ITokenEncryptionService? tokenEncryptionService)
    {
        _tokenEncryptionService = tokenEncryptionService;
    }

    public void Configure(EntityTypeBuilder<AppConnection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
               .WithMany(x => x.AppConnections)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedBy)
               .WithMany(x => x.CreatedAppConnections)
               .HasForeignKey(x => x.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedBy)
               .WithMany(x => x.UpdatedAppConnections)
               .HasForeignKey(x => x.UpdatedById)
               .OnDelete(DeleteBehavior.Restrict);

        // هنا تم استبدال الـ Protector بالـ Service الجديدة المتأمنة بـ AesGcm والـ Try-Catch الدفاعي
        builder.Property(x => x.AccessToken)
               .HasConversion(
                   rawString => _tokenEncryptionService != null ? _tokenEncryptionService.EncryptToken(rawString) : rawString,
                   encryptedString => _tokenEncryptionService != null ? _tokenEncryptionService.DecryptToken(encryptedString) : encryptedString)
               .IsRequired();

        builder.Property(x => x.RefreshToken)
               .HasConversion(
                   rawString => string.IsNullOrEmpty(rawString) ? null : (_tokenEncryptionService != null ? _tokenEncryptionService.EncryptToken(rawString) : rawString),
                   encryptedString => string.IsNullOrEmpty(encryptedString) ? null : (_tokenEncryptionService != null ? _tokenEncryptionService.DecryptToken(encryptedString) : encryptedString));
    }
}