using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnitPriceManuallyOverridden",
                table: "EstimateWorkItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PriceCapturedAt",
                table: "EstimateWorkItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourcePriceId",
                table: "EstimateWorkItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnitPriceManuallyOverridden",
                table: "EstimateMaterialItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PriceCapturedAt",
                table: "EstimateMaterialItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourcePriceId",
                table: "EstimateMaterialItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    KnowledgeMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConstructionWorkId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: false),
                    RegionCode = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogPrices", x => x.Id);
                    table.CheckConstraint("CK_CatalogPrices_EffectiveRange", "\"EffectiveUntil\" IS NULL OR \"EffectiveUntil\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_CatalogPrices_Target", "(\"TargetType\" = 'Material' AND \"KnowledgeMaterialId\" IS NOT NULL AND \"ConstructionWorkId\" IS NULL)\nOR (\"TargetType\" = 'ConstructionWork' AND \"ConstructionWorkId\" IS NOT NULL AND \"KnowledgeMaterialId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_CatalogPrices_ConstructionWorks_ConstructionWorkId",
                        column: x => x.ConstructionWorkId,
                        principalTable: "ConstructionWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogPrices_KnowledgeMaterials_KnowledgeMaterialId",
                        column: x => x.KnowledgeMaterialId,
                        principalTable: "KnowledgeMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogPriceHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogPriceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    KnowledgeMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConstructionWorkId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: false),
                    RegionCode = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PriceStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogPriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogPriceHistory_CatalogPrices_CatalogPriceId",
                        column: x => x.CatalogPriceId,
                        principalTable: "CatalogPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPriceHistory_CatalogPriceId",
                table: "CatalogPriceHistory",
                column: "CatalogPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPriceHistory_Material",
                table: "CatalogPriceHistory",
                columns: new[] { "TargetType", "KnowledgeMaterialId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPriceHistory_Work",
                table: "CatalogPriceHistory",
                columns: new[] { "TargetType", "ConstructionWorkId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPrices_ConstructionWorkId",
                table: "CatalogPrices",
                column: "ConstructionWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPrices_KnowledgeMaterialId",
                table: "CatalogPrices",
                column: "KnowledgeMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPrices_Material_Current",
                table: "CatalogPrices",
                columns: new[] { "TargetType", "KnowledgeMaterialId", "Currency", "Status", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPrices_RegionCode",
                table: "CatalogPrices",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPrices_SupplierName",
                table: "CatalogPrices",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogPrices_Work_Current",
                table: "CatalogPrices",
                columns: new[] { "TargetType", "ConstructionWorkId", "Currency", "Status", "EffectiveFrom" });

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "UX_CatalogPrices_OpenScope"
                ON "CatalogPrices" (
                    "TargetType",
                    COALESCE("KnowledgeMaterialId", '00000000-0000-0000-0000-000000000000'::uuid),
                    COALESCE("ConstructionWorkId", '00000000-0000-0000-0000-000000000000'::uuid),
                    "Currency",
                    COALESCE("RegionCode", ''),
                    COALESCE("SupplierId", '00000000-0000-0000-0000-000000000000'::uuid),
                    lower(COALESCE("SupplierName", ''))
                )
                WHERE "EffectiveUntil" IS NULL AND "Status" = 'Active';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_CatalogPrices_OpenScope\";");

            migrationBuilder.DropTable(
                name: "CatalogPriceHistory");

            migrationBuilder.DropTable(
                name: "CatalogPrices");

            migrationBuilder.DropColumn(
                name: "IsUnitPriceManuallyOverridden",
                table: "EstimateWorkItems");

            migrationBuilder.DropColumn(
                name: "PriceCapturedAt",
                table: "EstimateWorkItems");

            migrationBuilder.DropColumn(
                name: "SourcePriceId",
                table: "EstimateWorkItems");

            migrationBuilder.DropColumn(
                name: "IsUnitPriceManuallyOverridden",
                table: "EstimateMaterialItems");

            migrationBuilder.DropColumn(
                name: "PriceCapturedAt",
                table: "EstimateMaterialItems");

            migrationBuilder.DropColumn(
                name: "SourcePriceId",
                table: "EstimateMaterialItems");
        }
    }
}

#pragma warning restore CA1861
