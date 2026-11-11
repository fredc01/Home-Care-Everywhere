using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HomeCarePH.Migrations
{
    /// <inheritdoc />
    public partial class AddApplianceWarrantyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appliances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Brand = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ModelNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    WarrantyStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WarrantyEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appliances_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplianceDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplianceId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoredPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplianceDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplianceDocuments_Appliances_ApplianceId",
                        column: x => x.ApplianceId,
                        principalTable: "Appliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplianceDocuments_ApplianceId",
                table: "ApplianceDocuments",
                column: "ApplianceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appliances_PropertyId",
                table: "Appliances",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplianceDocuments");

            migrationBuilder.DropTable(
                name: "Appliances");
        }
    }
}
