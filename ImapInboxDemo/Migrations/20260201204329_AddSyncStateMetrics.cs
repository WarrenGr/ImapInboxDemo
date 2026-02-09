using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImapInboxDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncStateMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastRunNewMessages",
                table: "SyncState",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRunTime",
                table: "SyncState",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PauseBackfill",
                table: "SyncState",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRunNewMessages",
                table: "SyncState");

            migrationBuilder.DropColumn(
                name: "LastRunTime",
                table: "SyncState");

            migrationBuilder.DropColumn(
                name: "PauseBackfill",
                table: "SyncState");
        }
    }
}
