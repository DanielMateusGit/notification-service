using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseRecipientValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "channel",
                table: "notifications",
                newName: "recipient_channel");

            migrationBuilder.RenameColumn(
                name: "recipient",
                table: "notifications",
                newName: "recipient_value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "recipient_channel",
                table: "notifications",
                newName: "channel");

            migrationBuilder.RenameColumn(
                name: "recipient_value",
                table: "notifications",
                newName: "recipient");
        }
    }
}
