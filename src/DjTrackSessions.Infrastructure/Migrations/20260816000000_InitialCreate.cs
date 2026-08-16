using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DjTrackSessions.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "application_settings",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_application_settings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "jobs",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Identifier = table.Column<Guid>(type: "uuid", nullable: false),
                State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Progress = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CompletionMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                ErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_jobs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "job_notifications",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                JobId = table.Column<int>(type: "integer", nullable: false),
                JobIdentifier = table.Column<Guid>(type: "uuid", nullable: false),
                Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_job_notifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_job_notifications_jobs_JobId",
                    column: x => x.JobId,
                    principalTable: "jobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_application_settings_Key",
            table: "application_settings",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_job_notifications_JobId",
            table: "job_notifications",
            column: "JobId");

        migrationBuilder.CreateIndex(
            name: "IX_jobs_Identifier",
            table: "jobs",
            column: "Identifier",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "job_notifications");

        migrationBuilder.DropTable(
            name: "jobs");

        migrationBuilder.DropTable(
            name: "application_settings");
    }
}
