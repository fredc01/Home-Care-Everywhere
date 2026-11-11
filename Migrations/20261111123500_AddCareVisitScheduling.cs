using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeCarePH.Migrations;

public partial class AddCareVisitScheduling : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime?>("CompletedAtUtc", "CareVisits", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>("CompletionNotes", "CareVisits", type: "character varying(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<int>("ExpectedDurationMinutes", "CareVisits", type: "integer", nullable: false, defaultValue: 60);
        migrationBuilder.AddColumn<string>("Instructions", "CareVisits", type: "character varying(1200)", maxLength: 1200, nullable: true);
        migrationBuilder.AddColumn<DateTime?>("StartedAtUtc", "CareVisits", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<DateTime>("UpdatedAtUtc", "CareVisits", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("CompletedAtUtc", "CareVisits");
        migrationBuilder.DropColumn("CompletionNotes", "CareVisits");
        migrationBuilder.DropColumn("ExpectedDurationMinutes", "CareVisits");
        migrationBuilder.DropColumn("Instructions", "CareVisits");
        migrationBuilder.DropColumn("StartedAtUtc", "CareVisits");
        migrationBuilder.DropColumn("UpdatedAtUtc", "CareVisits");
    }
}
