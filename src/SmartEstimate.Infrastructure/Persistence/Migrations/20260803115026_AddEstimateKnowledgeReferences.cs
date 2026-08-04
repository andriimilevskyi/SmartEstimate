using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimateKnowledgeReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KnowledgeItemId",
                table: "EstimateWorkItems",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeItemId",
                table: "EstimateMaterialItems",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KnowledgeItemId",
                table: "EstimateWorkItems");

            migrationBuilder.DropColumn(
                name: "KnowledgeItemId",
                table: "EstimateMaterialItems");
        }
    }
}
