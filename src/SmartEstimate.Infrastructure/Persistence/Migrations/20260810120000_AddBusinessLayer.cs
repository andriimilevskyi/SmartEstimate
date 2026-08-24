using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartEstimate.Infrastructure.Persistence;

#nullable disable

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmartEstimateDbContext))]
    [Migration("20260810120000_AddBusinessLayer")]
    public partial class AddBusinessLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstimateObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TotalArea = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstimateObjects_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ObjectId",
                table: "Estimates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Estimates",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.Sql("""
                INSERT INTO "Customers" ("Id", "Name", "Phone", "Email", "Note", "CreatedAt", "UpdatedAt", "DeletedAt", "IsDeleted", "Version")
                SELECT
                    CONCAT(
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 1 FOR 8), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 9 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 13 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 17 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 21 FOR 12)
                    )::uuid,
                    'Замовник не вказаний',
                    NULL,
                    NULL,
                    'Створено автоматично під час міграції Business Layer.',
                    "CreatedAt",
                    "UpdatedAt",
                    NULL,
                    FALSE,
                    1
                FROM "Estimates";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "EstimateObjects" ("Id", "CustomerId", "Name", "ObjectType", "Address", "TotalArea", "Description", "CreatedAt", "UpdatedAt", "DeletedAt", "IsDeleted", "Version")
                SELECT
                    CONCAT(
                        SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 1 FOR 8), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 9 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 13 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 17 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 21 FOR 12)
                    )::uuid,
                    CONCAT(
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 1 FOR 8), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 9 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 13 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 17 FOR 4), '-',
                        SUBSTRING(MD5("Id"::text || ':legacy-customer') FROM 21 FOR 12)
                    )::uuid,
                    COALESCE(NULLIF("ObjectAddress", ''), 'Об''єкт ' || "EstimateNumber"),
                    "ObjectType",
                    "ObjectAddress",
                    "TotalArea",
                    NULL,
                    "CreatedAt",
                    "UpdatedAt",
                    NULL,
                    FALSE,
                    1
                FROM "Estimates";
                """);

            migrationBuilder.Sql("""
                UPDATE "Estimates"
                SET "ObjectId" = CONCAT(
                    SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 1 FOR 8), '-',
                    SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 9 FOR 4), '-',
                    SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 13 FOR 4), '-',
                    SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 17 FOR 4), '-',
                    SUBSTRING(MD5("Id"::text || ':legacy-object') FROM 21 FOR 12)
                )::uuid;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "Estimates",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(name: "ObjectType", table: "Estimates");
            migrationBuilder.DropColumn(name: "ObjectAddress", table: "Estimates");
            migrationBuilder.DropColumn(name: "TotalArea", table: "Estimates");

            migrationBuilder.CreateIndex(name: "IX_Customers_Name", table: "Customers", column: "Name");
            migrationBuilder.CreateIndex(name: "IX_Customers_Phone", table: "Customers", column: "Phone");
            migrationBuilder.CreateIndex(name: "IX_EstimateObjects_Address", table: "EstimateObjects", column: "Address");
            migrationBuilder.CreateIndex(name: "IX_EstimateObjects_CustomerId", table: "EstimateObjects", column: "CustomerId");
            migrationBuilder.CreateIndex(name: "IX_EstimateObjects_Name", table: "EstimateObjects", column: "Name");
            migrationBuilder.CreateIndex(name: "IX_EstimateObjects_ObjectType", table: "EstimateObjects", column: "ObjectType");
            migrationBuilder.CreateIndex(name: "IX_Estimates_ObjectId", table: "Estimates", column: "ObjectId");
            migrationBuilder.CreateIndex(name: "IX_Estimates_Status", table: "Estimates", column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimates_EstimateObjects_ObjectId",
                table: "Estimates",
                column: "ObjectId",
                principalTable: "EstimateObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.Sql("""
                UPDATE "Estimates" estimate
                SET
                    "ObjectType" = estimateObject."ObjectType",
                    "ObjectAddress" = estimateObject."Address",
                    "TotalArea" = estimateObject."TotalArea"
                FROM "EstimateObjects" estimateObject
                WHERE estimateObject."Id" = estimate."ObjectId";
                """);

            migrationBuilder.DropForeignKey(name: "FK_Estimates_EstimateObjects_ObjectId", table: "Estimates");
            migrationBuilder.DropIndex(name: "IX_Estimates_ObjectId", table: "Estimates");
            migrationBuilder.DropIndex(name: "IX_Estimates_Status", table: "Estimates");
            migrationBuilder.DropTable(name: "EstimateObjects");
            migrationBuilder.DropTable(name: "Customers");
            migrationBuilder.DropColumn(name: "ObjectId", table: "Estimates");
            migrationBuilder.DropColumn(name: "Status", table: "Estimates");
        }
    }
}
