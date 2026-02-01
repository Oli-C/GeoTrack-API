using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoTrack.API.Migrations;

public partial class AddTenantIdToGpsFixes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "tenant_id",
            table: "gps_fixes",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        // Backfill from vehicles table
        migrationBuilder.Sql(@"
UPDATE gps_fixes gf
SET tenant_id = v.tenant_id
FROM vehicles v
WHERE gf.vehicle_id = v.id;
");

        // Guard: ensure no rows left with empty tenant_id
        migrationBuilder.Sql(@"
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM gps_fixes WHERE tenant_id = '00000000-0000-0000-0000-000000000000') THEN
    RAISE EXCEPTION 'gps_fixes has rows with empty tenant_id after backfill';
  END IF;
END $$;
");

        migrationBuilder.CreateIndex(
            name: "ix_gpsfix_tenant_vehicle_devicetime",
            table: "gps_fixes",
            columns: new[] { "tenant_id", "vehicle_id", "device_time_utc" });

        migrationBuilder.AddForeignKey(
            name: "FK_gps_fixes_vehicles_tenant_id_vehicle_id",
            table: "gps_fixes",
            columns: new[] { "tenant_id", "vehicle_id" },
            principalTable: "vehicles",
            principalColumns: new[] { "tenant_id", "id" },
            onDelete: ReferentialAction.Cascade);

        // Optional: drop old non-tenant index if it exists
        migrationBuilder.DropIndex(
            name: "ix_gpsfix_vehicle_devicetime",
            table: "gps_fixes");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_gpsfix_vehicle_devicetime",
            table: "gps_fixes",
            columns: new[] { "vehicle_id", "device_time_utc" });

        migrationBuilder.DropForeignKey(
            name: "FK_gps_fixes_vehicles_tenant_id_vehicle_id",
            table: "gps_fixes");

        migrationBuilder.DropIndex(
            name: "ix_gpsfix_tenant_vehicle_devicetime",
            table: "gps_fixes");

        migrationBuilder.DropColumn(
            name: "tenant_id",
            table: "gps_fixes");
    }
}
