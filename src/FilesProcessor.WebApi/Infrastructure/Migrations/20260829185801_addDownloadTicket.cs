using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilesProcessor.WebApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addDownloadTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTickets_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTickets_ExpiresAt",
                table: "DownloadTickets",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTickets_FileId",
                table: "DownloadTickets",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTickets_Token",
                table: "DownloadTickets",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadTickets");
        }
    }
}
