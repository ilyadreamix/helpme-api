using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpMeApi.Migrations
{
    /// <inheritdoc />
    public partial class M07_09_23_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "TopicUserRelation");

            migrationBuilder.RenameTable(
                name: "ChatTopicRelation",
                newName: "TopicChatRelation");

            migrationBuilder.RenameIndex(
                name: "IX_UserTopicRelation_UsersId",
                table: "TopicUserRelation",
                newName: "IX_TopicUserRelation_UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatTopicRelation_TopicsId",
                table: "TopicChatRelation",
                newName: "IX_TopicChatRelation_TopicsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicUserRelation",
                table: "TopicUserRelation",
                columns: new[] { "TopicsId", "UsersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicChatRelation",
                table: "TopicChatRelation",
                columns: new[] { "ChatsId", "TopicsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TopicChatRelation_Chats_ChatsId",
                table: "TopicChatRelation",
                column: "ChatsId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicChatRelation_Topics_TopicsId",
                table: "TopicChatRelation",
                column: "TopicsId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicUserRelation_Topics_TopicsId",
                table: "TopicUserRelation",
                column: "TopicsId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicUserRelation_Users_UsersId",
                table: "TopicUserRelation",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopicChatRelation_Chats_ChatsId",
                table: "TopicChatRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicChatRelation_Topics_TopicsId",
                table: "TopicChatRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicUserRelation_Topics_TopicsId",
                table: "TopicUserRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicUserRelation_Users_UsersId",
                table: "TopicUserRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicUserRelation",
                table: "TopicUserRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicChatRelation",
                table: "TopicChatRelation");

            migrationBuilder.RenameTable(
                name: "TopicUserRelation",
                newName: "UserTopicRelation");

            migrationBuilder.RenameTable(
                name: "TopicChatRelation",
                newName: "ChatTopicRelation");

            migrationBuilder.RenameIndex(
                name: "IX_TopicUserRelation_UsersId",
                table: "UserTopicRelation",
                newName: "IX_UserTopicRelation_UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_TopicChatRelation_TopicsId",
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
    }
}
