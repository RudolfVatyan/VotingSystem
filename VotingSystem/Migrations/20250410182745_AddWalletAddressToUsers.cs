using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletAddressToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasVoted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasVoted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "Users");
        }
    }
}
