using DjTrackSessions.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DjTrackSessions.Infrastructure.Persistence.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Identifier)
            .IsRequired();
        builder.HasIndex(job => job.Identifier).IsUnique();
        builder.Property(job => job.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(job => job.Progress)
            .IsRequired();
        builder.Property(job => job.CreatedAtUtc)
            .IsRequired();
        builder.Property(job => job.StartedAtUtc);
        builder.Property(job => job.CompletedAtUtc);
        builder.Property(job => job.CompletionMessage)
            .HasMaxLength(2000);
        builder.Property(job => job.ErrorCode)
            .HasMaxLength(200);
        builder.Property(job => job.ErrorMessage)
            .HasMaxLength(2000);
    }
}
