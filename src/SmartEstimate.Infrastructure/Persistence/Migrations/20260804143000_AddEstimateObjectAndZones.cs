using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartEstimate.Infrastructure.Persistence;

#nullable disable

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmartEstimateDbContext))]
    [Migration("20260804143000_AddEstimateObjectAndZones")]
    public partial class AddEstimateObjectAndZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObjectType",
                table: "Estimates",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Apartment");

            migrationBuilder.AddColumn<string>(
                name: "ObjectAddress",
                table: "Estimates",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalArea",
                table: "Estimates",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstimateZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateZones", zone => zone.Id);
                    table.ForeignKey(
                        name: "FK_EstimateZones_Estimates_EstimateId",
                        column: zone => zone.EstimateId,
                        principalTable: "Estimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ZoneId",
                table: "EstimateWorkItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ZoneId",
                table: "EstimateMaterialItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO "EstimateZones" ("Id", "EstimateId", "Name", "SortOrder", "CreatedAt", "UpdatedAt")
                SELECT
                    CONCAT(
                        SUBSTRING(MD5("Id"::text || ':default-zone') FROM 1 FOR 8), '-',
                        SUBSTRING(MD5("Id"::text || ':default-zone') FROM 9 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':default-zone') FROM 13 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':default-zone') FROM 17 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':default-zone') FROM 21 FOR 12)
                    )::uuid,
                    "Id",
                    'Основна зона',
                    0,
                    "CreatedAt",
                    "UpdatedAt"
                FROM "Estimates";
                """);

            migrationBuilder.Sql("""
                UPDATE "EstimateWorkItems" item
                SET "ZoneId" = zone."Id"
                FROM "EstimateZones" zone
                WHERE zone."EstimateId" = item."EstimateId";
                """);

            migrationBuilder.Sql("""
                UPDATE "EstimateMaterialItems" item
                SET "ZoneId" = zone."Id"
                FROM "EstimateZones" zone
                WHERE zone."EstimateId" = item."EstimateId";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ZoneId",
                table: "EstimateWorkItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ZoneId",
                table: "EstimateMaterialItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstimateZones_EstimateId",
                table: "EstimateZones",
                column: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateZones_EstimateId_Name",
                table: "EstimateZones",
                columns: ["EstimateId", "Name"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstimateWorkItems_ZoneId",
                table: "EstimateWorkItems",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateMaterialItems_ZoneId",
                table: "EstimateMaterialItems",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstimateWorkItems_EstimateZones_ZoneId",
                table: "EstimateWorkItems",
                column: "ZoneId",
                principalTable: "EstimateZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstimateMaterialItems_EstimateZones_ZoneId",
                table: "EstimateMaterialItems",
                column: "ZoneId",
                principalTable: "EstimateZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstimateWorkItems_EstimateZones_ZoneId",
                table: "EstimateWorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_EstimateMaterialItems_EstimateZones_ZoneId",
                table: "EstimateMaterialItems");

            migrationBuilder.DropTable(name: "EstimateZones");

            migrationBuilder.DropColumn(name: "ZoneId", table: "EstimateWorkItems");
            migrationBuilder.DropColumn(name: "ZoneId", table: "EstimateMaterialItems");
            migrationBuilder.DropColumn(name: "ObjectType", table: "Estimates");
            migrationBuilder.DropColumn(name: "ObjectAddress", table: "Estimates");
            migrationBuilder.DropColumn(name: "TotalArea", table: "Estimates");
        }
    }
}
