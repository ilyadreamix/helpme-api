using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpMeApi.Migrations
{
    /// <inheritdoc />
    public partial class M07_09_23_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "BannedUserIds",
                table: "Chats",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::uuid[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedUserIds",
                table: "Chats");
        }
    }
}
