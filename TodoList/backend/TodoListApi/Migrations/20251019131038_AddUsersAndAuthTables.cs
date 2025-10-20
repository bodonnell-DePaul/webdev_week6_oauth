using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndAuthTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Todos",
                type: "TEXT",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Picture = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProviderUserId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "CreatedDate", "DueDate", "UserId" },
                values: new object[] { new DateTime(2025, 10, 16, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4435), new DateTime(2025, 10, 30, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4441), null });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CreatedDate", "DueDate", "UserId" },
                values: new object[] { new DateTime(2025, 10, 17, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4459), new DateTime(2025, 10, 20, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4460), null });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CreatedDate", "DueDate", "UserId" },
                values: new object[] { new DateTime(2025, 10, 18, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4465), new DateTime(2025, 10, 23, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4465), null });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "CreatedDate", "UserId" },
                values: new object[] { new DateTime(2025, 10, 19, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4470), null });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "5",
                columns: new[] { "CreatedDate", "UserId" },
                values: new object[] { new DateTime(2025, 10, 19, 13, 10, 37, 673, DateTimeKind.Utc).AddTicks(4475), null });

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId",
                table: "Todos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Provider_ProviderUserId",
                table: "Users",
                columns: new[] { "Provider", "ProviderUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Users_UserId",
                table: "Todos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Users_UserId",
                table: "Todos");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Todos");

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "CreatedDate", "DueDate" },
                values: new object[] { new DateTime(2025, 10, 11, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4651), new DateTime(2025, 10, 25, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4658) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CreatedDate", "DueDate" },
                values: new object[] { new DateTime(2025, 10, 12, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4688), new DateTime(2025, 10, 15, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4689) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CreatedDate", "DueDate" },
                values: new object[] { new DateTime(2025, 10, 13, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4701), new DateTime(2025, 10, 18, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4701) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedDate",
                value: new DateTime(2025, 10, 14, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4713));

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedDate",
                value: new DateTime(2025, 10, 14, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4728));
        }
    }
}
