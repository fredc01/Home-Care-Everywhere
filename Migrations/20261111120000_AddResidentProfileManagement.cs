using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeCarePH.Migrations;

public partial class AddResidentProfileManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"ArchivedAtUtc\" timestamp with time zone NULL;");
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"CareNotes\" character varying(4000) NULL;");
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"CareRequirements\" character varying(2000) NULL;");
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"EmergencyContactName\" character varying(120) NULL;");
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"EmergencyContactNumber\" character varying(40) NULL;");
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"EmergencyContactRelationship\" character varying(80) NULL;");
        migrationBuilder.Sql("ALTER TABLE \"Residents\" ADD COLUMN IF NOT EXISTS \"IsActive\" boolean NOT NULL DEFAULT TRUE;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("ArchivedAtUtc", "Residents");
        migrationBuilder.DropColumn("CareNotes", "Residents");
        migrationBuilder.DropColumn("CareRequirements", "Residents");
        migrationBuilder.DropColumn("EmergencyContactName", "Residents");
        migrationBuilder.DropColumn("EmergencyContactNumber", "Residents");
        migrationBuilder.DropColumn("EmergencyContactRelationship", "Residents");
        migrationBuilder.DropColumn("IsActive", "Residents");
    }
}
