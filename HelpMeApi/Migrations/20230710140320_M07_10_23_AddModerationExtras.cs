using System.Collections.Generic;
using HelpMeApi.Common.Object;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpMeApi.Migrations
{
    /// <inheritdoc />
    public partial class M07_10_23_AddModerationExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Extras", "Moderations");
            migrationBuilder.AddColumn<List<Extra>>(
                name: "Extras",
                table: "Moderations",
                type: "jsonb[]",
                nullable: false,
                defaultValue: new List<Extra>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Extras", "Moderations");
            migrationBuilder.AddColumn<List<Extra>>(
                name: "Extras",
                table: "Moderations",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());
        }
    }
}
