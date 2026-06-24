using AI_genda_API.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI_genda_API.Presistiences.EntitiesConfiguration;

public class AppConnectionConfiguration : IEntityTypeConfiguration<AppConnection>
{
    private readonly IDataProtectionProvider? _dataProtectionProvider;

    // 1. تم إضافة الـ Parameterless Constructor هنا لحل مشكلة الـ Assembly Scanning وقت الـ Migration
    public AppConnectionConfiguration()
    {
        _dataProtectionProvider = null;
    }

    public AppConnectionConfiguration(IDataProtectionProvider? dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    public void Configure(EntityTypeBuilder<AppConnection> builder)
    {
        IDataProtector? protector = _dataProtectionProvider?.CreateProtector("AppConnection.Tokens");

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

        builder.Property(x => x.AccessToken)
               .HasConversion(
                   rawString => protector != null ? protector.Protect(rawString) : rawString,
                   encryptedString => protector != null ? protector.Unprotect(encryptedString) : encryptedString)
               .IsRequired();

        builder.Property(x => x.RefreshToken)
               .HasConversion(
                   rawString => string.IsNullOrEmpty(rawString) ? null : (protector != null ? protector.Protect(rawString) : rawString),
                   encryptedString => string.IsNullOrEmpty(encryptedString) ? null : (protector != null ? protector.Unprotect(encryptedString) : encryptedString));
    }
}