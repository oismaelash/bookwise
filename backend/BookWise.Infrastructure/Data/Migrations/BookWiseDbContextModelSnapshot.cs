using System;
using BookWise.Domain.Entities;
using BookWise.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookWise.Infrastructure.Data.Migrations;

[DbContextAttribute(typeof(BookWiseDbContext))]
public partial class BookWiseDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity<Author>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<string>("Biography")
                .HasColumnType("text")
                .HasColumnName("biography");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            b.Property<DateTime?>("BirthDate")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("birth_date");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("boolean")
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)")
                .HasColumnName("name");

            b.Property<string>("Nationality")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("nationality");

            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");

            b.HasKey("Id");

            b.HasIndex("Name");

            b.ToTable("authors");

            b.HasQueryFilter(a => a.IsActive);
        });

        modelBuilder.Entity<Genre>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<string>("Description")
                .HasColumnType("text")
                .HasColumnName("description");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("boolean")
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("name");

            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");

            b.HasKey("Id");

            b.HasIndex("Name")
                .IsUnique();

            b.ToTable("genres");

            b.HasQueryFilter(g => g.IsActive);
        });

        modelBuilder.Entity<Book>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<int>("AuthorId")
                .HasColumnType("integer")
                .HasColumnName("author_id");

            b.Property<string>("CoverImageUrl")
                .HasColumnType("text")
                .HasColumnName("cover_image_url");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            b.Property<string>("Description")
                .HasColumnType("text")
                .HasColumnName("description");

            b.Property<int>("GenreId")
                .HasColumnType("integer")
                .HasColumnName("genre_id");

            b.Property<string>("ISBN")
                .HasMaxLength(13)
                .HasColumnType("character varying(13)")
                .HasColumnName("isbn");

            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("boolean")
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            b.Property<int>("PublicationYear")
                .HasColumnType("integer")
                .HasColumnName("publication_year");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("character varying(300)")
                .HasColumnName("title");

            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");

            b.HasKey("Id");

            b.HasIndex("AuthorId");

            b.HasIndex("GenreId");

            b.HasIndex("ISBN")
                .IsUnique()
                .HasFilter("\"isbn\" IS NOT NULL");

            b.HasIndex("Title");

            b.ToTable("books");

            b.HasQueryFilter(b => b.IsActive);
        });

        modelBuilder.Entity<Book>(b =>
        {
            b.HasOne("BookWise.Domain.Entities.Author", "Author")
                .WithMany("Books")
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("BookWise.Domain.Entities.Genre", "Genre")
                .WithMany("Books")
                .HasForeignKey("GenreId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Author");
            b.Navigation("Genre");
        });

        modelBuilder.Entity<Author>(b =>
        {
            b.Navigation("Books");
        });

        modelBuilder.Entity<Genre>(b =>
        {
            b.Navigation("Books");
        });
    }
}
