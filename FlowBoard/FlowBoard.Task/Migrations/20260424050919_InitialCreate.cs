using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowBoard.Card.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cards",
                columns: table => new
                {
                    card_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    list_id = table.Column<int>(type: "integer", nullable: false),
                    board_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assignee_id = table.Column<string>(type: "text", nullable: true),
                    created_by_id = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    cover_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cards", x => x.card_id);
                });

            migrationBuilder.CreateTable(
                name: "card_activities",
                columns: table => new
                {
                    activity_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    card_id = table.Column<int>(type: "integer", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    detail = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_activities", x => x.activity_id);
                    table.ForeignKey(
                        name: "FK_card_activities_cards_card_id",
                        column: x => x.card_id,
                        principalTable: "cards",
                        principalColumn: "card_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_activities_card_id",
                table: "card_activities",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_cards_assignee_id",
                table: "cards",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "IX_cards_board_id",
                table: "cards",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "IX_cards_due_date",
                table: "cards",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_cards_list_id_position",
                table: "cards",
                columns: new[] { "list_id", "position" },
                unique: true,
                filter: "is_archived = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_activities");

            migrationBuilder.DropTable(
                name: "cards");
        }
    }
}
