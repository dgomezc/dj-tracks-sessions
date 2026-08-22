using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DjTrackSessions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistExplicitReadOnlyCatalogScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_tracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RootType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ByteLength = table.Column<long>(type: "bigint", nullable: false),
                    LastObservedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsMissing = table.Column<bool>(type: "boolean", nullable: false),
                    LastErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_tracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "session_catalog_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelativeFolderPath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AudioRelativePaths = table.Column<string[]>(type: "text[]", nullable: false),
                    TracklistRelativePaths = table.Column<string[]>(type: "text[]", nullable: false),
                    ArtworkRelativePaths = table.Column<string[]>(type: "text[]", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LastObservedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    Issues = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_catalog_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_track_metadata",
                columns: table => new
                {
                    CatalogTrackId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Artists = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Album = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Year = table.Column<long>(type: "bigint", nullable: true),
                    Genre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PersonalGenre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BeatsPerMinute = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Key = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    BitrateKbps = table.Column<int>(type: "integer", nullable: true),
                    SampleRateHz = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_track_metadata", x => x.CatalogTrackId);
                    table.ForeignKey(
                        name: "FK_catalog_track_metadata_catalog_tracks_CatalogTrackId",
                        column: x => x.CatalogTrackId,
                        principalTable: "catalog_tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_tracks_RootType_ContentHash",
                table: "catalog_tracks",
                columns: new[] { "RootType", "ContentHash" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_tracks_RootType_RelativePath",
                table: "catalog_tracks",
                columns: new[] { "RootType", "RelativePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_catalog_items_RelativeFolderPath",
                table: "session_catalog_items",
                column: "RelativeFolderPath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_track_metadata");

            migrationBuilder.DropTable(
                name: "session_catalog_items");

            migrationBuilder.DropTable(
                name: "catalog_tracks");
        }
    }
}
