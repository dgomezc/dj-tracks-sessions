using DjTrackSessions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DjTrackSessions.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("DjTrackSessions.Domain.Configuration.ApplicationSetting", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<string>("Key")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<string>("Value")
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)");

            b.HasKey("Id");
            b.HasIndex("Key").IsUnique();
            b.ToTable("application_settings", (string)null);
        });

        modelBuilder.Entity("DjTrackSessions.Domain.Jobs.Job", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<DateTimeOffset?>("CompletedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("CompletionMessage")
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)");

            b.Property<DateTimeOffset>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("ErrorCode")
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<string>("ErrorMessage")
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)");

            b.Property<Guid>("Identifier")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset?>("StartedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Progress")
                .HasColumnType("integer");

            b.Property<string>("State")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.HasKey("Id");
            b.HasIndex("Identifier").IsUnique();
            b.ToTable("jobs", (string)null);
        });

        modelBuilder.Entity("DjTrackSessions.Domain.Notifications.JobNotification", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<DateTimeOffset>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Kind")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<Guid>("JobIdentifier")
                .HasColumnType("uuid");

            b.Property<int>("JobId")
                .HasColumnType("integer");

            b.Property<string>("Message")
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.HasKey("Id");
            b.HasIndex("JobId");
            b.ToTable("job_notifications", (string)null);

            b.HasOne("DjTrackSessions.Domain.Jobs.Job", null)
                .WithMany()
                .HasForeignKey("JobId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}
