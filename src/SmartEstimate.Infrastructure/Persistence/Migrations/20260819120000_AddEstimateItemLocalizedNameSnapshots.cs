using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartEstimate.Infrastructure.Persistence;

#nullable disable

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmartEstimateDbContext))]
    [Migration("20260819120000_AddEstimateItemLocalizedNameSnapshots")]
    public partial class AddEstimateItemLocalizedNameSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddSnapshotColumns(migrationBuilder, "EstimateMaterialItems");
            AddSnapshotColumns(migrationBuilder, "EstimateWorkItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropSnapshotColumns(migrationBuilder, "EstimateMaterialItems");
            DropSnapshotColumns(migrationBuilder, "EstimateWorkItems");
        }

        private static void AddSnapshotColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameSnapshotUk",
                table: table,
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameSnapshotEn",
                table: table,
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameSnapshotDe",
                table: table,
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameSource",
                table: table,
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Legacy");
        }

        private static void DropSnapshotColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.DropColumn(
                name: "NameSnapshotUk",
                table: table);

            migrationBuilder.DropColumn(
                name: "NameSnapshotEn",
                table: table);

            migrationBuilder.DropColumn(
                name: "NameSnapshotDe",
                table: table);

            migrationBuilder.DropColumn(
                name: "NameSource",
                table: table);
        }
    }
}
