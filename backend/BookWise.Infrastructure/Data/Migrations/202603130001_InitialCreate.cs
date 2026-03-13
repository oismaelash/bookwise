using System;
using BookWise.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookWise.Infrastructure.Data.Migrations;

[DbContextAttribute(typeof(BookWiseDbContext))]
[Migration("202603130001_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "authors",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                biography = table.Column<string>(type: "text", nullable: true),
                nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_authors", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "genres",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_genres", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "books",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                publication_year = table.Column<int>(type: "integer", nullable: false),
                isbn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                author_id = table.Column<int>(type: "integer", nullable: false),
                genre_id = table.Column<int>(type: "integer", nullable: false),
                cover_image_url = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_books", x => x.id);
                table.ForeignKey(
                    name: "FK_books_authors_author_id",
                    column: x => x.author_id,
                    principalTable: "authors",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_books_genres_genre_id",
                    column: x => x.genre_id,
                    principalTable: "genres",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_authors_name",
            table: "authors",
            column: "name");

        migrationBuilder.CreateIndex(
            name: "IX_genres_name",
            table: "genres",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_books_author_id",
            table: "books",
            column: "author_id");

        migrationBuilder.CreateIndex(
            name: "IX_books_genre_id",
            table: "books",
            column: "genre_id");

        migrationBuilder.CreateIndex(
            name: "IX_books_isbn",
            table: "books",
            column: "isbn",
            unique: true,
            filter: "\"isbn\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_books_title",
            table: "books",
            column: "title");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "books");

        migrationBuilder.DropTable(
            name: "authors");

        migrationBuilder.DropTable(
            name: "genres");
    }
}
