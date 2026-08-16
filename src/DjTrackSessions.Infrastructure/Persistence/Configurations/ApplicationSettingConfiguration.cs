using DjTrackSessions.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjTrackSessions.Infrastructure.Persistence.Configurations;

public sealed class ApplicationSettingConfiguration : IEntityTypeConfiguration<ApplicationSetting>
{
    public void Configure(EntityTypeBuilder<ApplicationSetting> builder)
    {
        builder.ToTable("application_settings");
        builder.HasKey(setting => setting.Id);
        builder.Property(setting => setting.Key)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(setting => setting.Value)
            .HasMaxLength(2000)
            .IsRequired();
        builder.HasIndex(setting => setting.Key).IsUnique();
    }
}
