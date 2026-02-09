using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImapInboxDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagsAndSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "EmailMessages");

            migrationBuilder.AddColumn<string>(
                name: "Flags",
                table: "EmailMessages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "EmailMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flags",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "EmailMessages");

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "EmailMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
