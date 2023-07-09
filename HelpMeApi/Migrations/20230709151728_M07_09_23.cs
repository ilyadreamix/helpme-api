using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpMeApi.Migrations
{
    /// <inheritdoc />
    public partial class M07_09_23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatTopic_Chats_ChatsId",
                table: "ChatTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatTopic_Topics_TopicsId",
                table: "ChatTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTopic_Topics_TopicsId",
                table: "UserTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTopic_Users_UsersId",
                table: "UserTopic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTopic",
                table: "UserTopic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatTopic",
                table: "ChatTopic");

            migrationBuilder.RenameTable(
                name: "UserTopic",
                newName: "UserTopicRelation");

            migrationBuilder.RenameTable(
                name: "ChatTopic",
                newName: "ChatTopicRelation");

            migrationBuilder.RenameIndex(
                name: "IX_UserTopic_UsersId",
                table: "UserTopicRelation",
                newName: "IX_UserTopicRelation_UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatTopic_TopicsId",
                table: "ChatTopicRelation",
                newName: "IX_ChatTopicRelation_TopicsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTopicRelation",
                table: "UserTopicRelation",
                columns: new[] { "TopicsId", "UsersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatTopicRelation",
                table: "ChatTopicRelation",
                columns: new[] { "ChatsId", "TopicsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatTopicRelation_Chats_ChatsId",
                table: "ChatTopicRelation",
                column: "ChatsId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatTopicRelation_Topics_TopicsId",
                table: "ChatTopicRelation",
                column: "TopicsId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTopicRelation_Topics_TopicsId",
                table: "UserTopicRelation",
                column: "TopicsId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTopicRelation_Users_UsersId",
                table: "UserTopicRelation",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatTopicRelation_Chats_ChatsId",
                table: "ChatTopicRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatTopicRelation_Topics_TopicsId",
                table: "ChatTopicRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTopicRelation_Topics_TopicsId",
                table: "UserTopicRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTopicRelation_Users_UsersId",
                table: "UserTopicRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTopicRelation",
                table: "UserTopicRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatTopicRelation",
                table: "ChatTopicRelation");

            migrationBuilder.RenameTable(
                name: "UserTopicRelation",
                newName: "UserTopic");

            migrationBuilder.RenameTable(
                name: "ChatTopicRelation",
                newName: "ChatTopic");

            migrationBuilder.RenameIndex(
                name: "IX_UserTopicRelation_UsersId",
                table: "UserTopic",
                newName: "IX_UserTopic_UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatTopicRelation_TopicsId",
                table: "ChatTopic",
                newName: "IX_ChatTopic_TopicsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTopic",
                table: "UserTopic",
                columns: new[] { "TopicsId", "UsersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatTopic",
                table: "ChatTopic",
                columns: new[] { "ChatsId", "TopicsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatTopic_Chats_ChatsId",
                table: "ChatTopic",
                column: "ChatsId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatTopic_Topics_TopicsId",
                table: "ChatTopic",
                column: "TopicsId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTopic_Topics_TopicsId",
                table: "UserTopic",
                column: "TopicsId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTopic_Users_UsersId",
                table: "UserTopic",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
