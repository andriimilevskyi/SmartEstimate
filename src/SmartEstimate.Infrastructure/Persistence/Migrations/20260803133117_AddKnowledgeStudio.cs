using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeStudio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameUk = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeCategories_KnowledgeCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "KnowledgeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NameUk = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConstructionWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameUk = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tags = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstructionWorks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstructionWorks_KnowledgeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "KnowledgeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConstructionWorks_MeasurementUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "MeasurementUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameUk = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tags = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeMaterials_KnowledgeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "KnowledgeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KnowledgeMaterials_MeasurementUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "MeasurementUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionWorks_CategoryId",
                table: "ConstructionWorks",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionWorks_NameUk",
                table: "ConstructionWorks",
                column: "NameUk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionWorks_Status",
                table: "ConstructionWorks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionWorks_UnitId",
                table: "ConstructionWorks",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeCategories_NameUk",
                table: "KnowledgeCategories",
                column: "NameUk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeCategories_ParentCategoryId",
                table: "KnowledgeCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeCategories_Status",
                table: "KnowledgeCategories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeMaterials_CategoryId",
                table: "KnowledgeMaterials",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeMaterials_NameUk",
                table: "KnowledgeMaterials",
                column: "NameUk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeMaterials_Status",
                table: "KnowledgeMaterials",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeMaterials_UnitId",
                table: "KnowledgeMaterials",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnits_NameUk",
                table: "MeasurementUnits",
                column: "NameUk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnits_Status",
                table: "MeasurementUnits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementUnits_Symbol",
                table: "MeasurementUnits",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConstructionWorks");

            migrationBuilder.DropTable(
                name: "KnowledgeMaterials");

            migrationBuilder.DropTable(
                name: "KnowledgeCategories");

            migrationBuilder.DropTable(
                name: "MeasurementUnits");
        }
    }
}
