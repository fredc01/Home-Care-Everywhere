using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HomeCarePH.Migrations;

public partial class AddCareTeamManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("Availability", "Caregivers", maxLength: 1200, nullable: true);
        migrationBuilder.AddColumn<string>("Email", "Caregivers", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<string>("Notes", "Caregivers", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<string>("Qualifications", "Caregivers", maxLength: 1200, nullable: true);
        migrationBuilder.AddColumn<string>("Skills", "Caregivers", maxLength: 1200, nullable: true);
        migrationBuilder.AddColumn<string>("Specializations", "Caregivers", maxLength: 1200, nullable: true);
        migrationBuilder.CreateTable(name: "CaregiverAssignments", columns: table => new
        {
            Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            CaregiverId = table.Column<int>(type: "integer", nullable: false),
            HouseholdId = table.Column<int>(type: "integer", nullable: false),
            ResidentId = table.Column<int>(type: "integer", nullable: true),
            Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
            AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            IsActive = table.Column<bool>(type: "boolean", nullable: false),
            EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_CaregiverAssignments", x => x.Id);
            table.ForeignKey("FK_CaregiverAssignments_Caregivers_CaregiverId", x => x.CaregiverId, "Caregivers", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_CaregiverAssignments_Households_HouseholdId", x => x.HouseholdId, "Households", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_CaregiverAssignments_Residents_ResidentId", x => x.ResidentId, "Residents", "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex(name: "IX_CaregiverAssignments_CaregiverId_HouseholdId_ResidentId_IsActive", table: "CaregiverAssignments", columns: new[] { "CaregiverId", "HouseholdId", "ResidentId", "IsActive" });
        migrationBuilder.CreateIndex(name: "IX_CaregiverAssignments_HouseholdId", table: "CaregiverAssignments", column: "HouseholdId");
        migrationBuilder.CreateIndex(name: "IX_CaregiverAssignments_ResidentId", table: "CaregiverAssignments", column: "ResidentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CaregiverAssignments");
        migrationBuilder.DropColumn("Availability", "Caregivers");
        migrationBuilder.DropColumn("Email", "Caregivers");
        migrationBuilder.DropColumn("Notes", "Caregivers");
        migrationBuilder.DropColumn("Qualifications", "Caregivers");
        migrationBuilder.DropColumn("Skills", "Caregivers");
        migrationBuilder.DropColumn("Specializations", "Caregivers");
    }
}
