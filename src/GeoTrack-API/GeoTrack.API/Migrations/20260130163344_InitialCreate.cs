using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gps_fixes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    speed_kph = table.Column<double>(type: "double precision", precision: 9, scale: 3, nullable: true),
                    heading_degrees = table.Column<double>(type: "double precision", precision: 9, scale: 3, nullable: true),
                    accuracy_meters = table.Column<double>(type: "double precision", precision: 9, scale: 3, nullable: true),
                    altitude_meters = table.Column<double>(type: "double precision", precision: 9, scale: 3, nullable: true),
                    odometer_km = table.Column<double>(type: "double precision", precision: 14, scale: 3, nullable: true),
                    quality = table.Column<int>(type: "integer", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    device_time_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    device_sequence = table.Column<long>(type: "bigint", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gps_fixes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    external_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => new { x.tenant_id, x.id });
                });

            migrationBuilder.CreateTable(
                name: "vehicle_latest_location",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gps_fix_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_time_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_sequence = table.Column<long>(type: "bigint", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    speed_kph = table.Column<double>(type: "double precision", nullable: true),
                    heading_degrees = table.Column<double>(type: "double precision", nullable: true),
                    accuracy_meters = table.Column<double>(type: "double precision", nullable: true),
                    route_schedule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_latest_location", x => new { x.tenant_id, x.vehicle_id });
                    table.ForeignKey(
                        name: "FK_vehicle_latest_location_vehicles_tenant_id_vehicle_id",
                        columns: x => new { x.tenant_id, x.vehicle_id },
                        principalTable: "vehicles",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gpsfix_receivedat",
                table: "gps_fixes",
                column: "received_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_gpsfix_vehicle_devicetime",
                table: "gps_fixes",
                columns: new[] { "vehicle_id", "device_time_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_tenants_name",
                table: "tenants",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vll_tenant_receivedat",
                table: "vehicle_latest_location",
                columns: new[] { "tenant_id", "received_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_vll_tenant_vehicle",
                table: "vehicle_latest_location",
                columns: new[] { "tenant_id", "vehicle_id" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tenant",
                table: "vehicles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ux_vehicle_tenant_registration_number",
                table: "vehicles",
                columns: new[] { "tenant_id", "registration_number" },
                unique: true,
                filter: "registration_number IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gps_fixes");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "vehicle_latest_location");

            migrationBuilder.DropTable(
                name: "vehicles");
        }
    }
}
