using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalTwinPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updates01032026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Data",
                table: "TelemetryData",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()));

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Parameters",
                table: "SimulationStates",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldDefaultValue: new Dictionary<string, object>());

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Metrics",
                table: "SimulationStates",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldDefaultValue: new Dictionary<string, object>());

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "SimulationStates",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Configuration",
                table: "ProductionLines",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()));

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Properties",
                table: "Machines",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()));

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Configuration",
                table: "Machines",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "SimulationStates");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Data",
                table: "TelemetryData",
                type: "jsonb",
                nullable: false,
                defaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()),
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Parameters",
                table: "SimulationStates",
                type: "jsonb",
                nullable: false,
                defaultValue: new Dictionary<string, object>(),
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "Metrics",
                table: "SimulationStates",
                type: "jsonb",
                nullable: false,
                defaultValue: new Dictionary<string, object>(),
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Configuration",
                table: "ProductionLines",
                type: "jsonb",
                nullable: false,
                defaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()),
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Properties",
                table: "Machines",
                type: "jsonb",
                nullable: false,
                defaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()),
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Configuration",
                table: "Machines",
                type: "jsonb",
                nullable: false,
                defaultValue: System.Text.Json.JsonDocument.Parse("{}", new System.Text.Json.JsonDocumentOptions()),
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");
        }
    }
}
