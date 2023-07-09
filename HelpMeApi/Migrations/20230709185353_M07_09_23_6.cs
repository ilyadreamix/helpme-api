using System;
using System.Collections.Generic;
using HelpMeApi.Chat.Entity.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpMeApi.Migrations
{
    /// <inheritdoc />
    public partial class M07_09_23_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedUserIds",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "InvitedUserIds",
                table: "Chats");

            migrationBuilder.AddColumn<List<ChatEntityBan>>(
                name: "BannedUsers",
                table: "Chats",
                type: "jsonb[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::jsonb[]");

            migrationBuilder.AddColumn<List<ChatEntityInvitation>>(
                name: "InvitedUsers",
                table: "Chats",
                type: "jsonb[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::jsonb[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedUsers",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "InvitedUsers",
                table: "Chats");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "BannedUserIds",
                table: "Chats",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "InvitedUserIds",
                table: "Chats",
                type: "uuid[]",
                nullable: false);
        }
    }
}
