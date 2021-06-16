﻿// <auto-generated />
using System;
using Ixfleura.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ixfleura.Data.Migrations
{
    [DbContext(typeof(IxfleuraDbContext))]
    partial class IxfleuraDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Ixfleura.Data.Entities.Campaign", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("AdvocateId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("advocate_id");

                    b.Property<decimal>("CandidateId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("candidate_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<string>("Type")
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_campaigns");

                    b.ToTable("campaigns");

                    b.HasCheckConstraint("campaigns_type_lowercase_ck", "type = lower(type)");
                });

            modelBuilder.Entity("Ixfleura.Data.Entities.Suggestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("message_id");

                    b.Property<decimal>("SuggesterId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("suggester_id");

                    b.HasKey("Id")
                        .HasName("pk_suggestions");

                    b.ToTable("suggestions");
                });

            modelBuilder.Entity("Ixfleura.Data.Entities.Tag", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Content")
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset>("EditedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("edited_at");

                    b.Property<long>("Uses")
                        .HasColumnType("bigint")
                        .HasColumnName("uses");

                    b.HasKey("GuildId", "Name")
                        .HasName("pk_tags");

                    b.ToTable("tags");
                });
#pragma warning restore 612, 618
        }
    }
}
