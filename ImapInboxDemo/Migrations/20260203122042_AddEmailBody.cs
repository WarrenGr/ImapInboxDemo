using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImapInboxDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailBody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "EmailMessages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "EmailMessages");
        }
    }
}
