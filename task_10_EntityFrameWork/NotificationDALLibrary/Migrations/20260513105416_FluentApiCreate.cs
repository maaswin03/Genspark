using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class FluentApiCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_ReceiverId",
                table: "notifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SendedAt",
                table: "notifications",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_RECEIVER_ID",
                table: "notifications",
                column: "ReceiverId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RECEIVER_ID",
                table: "notifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SendedAt",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_ReceiverId",
                table: "notifications",
                column: "ReceiverId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
