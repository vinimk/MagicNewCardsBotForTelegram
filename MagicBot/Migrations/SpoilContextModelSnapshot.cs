﻿// <auto-generated />
using MagicBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace MagicBot.Migrations
{
    [DbContext(typeof(SpoilDbContext))]
    partial class SpoilContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("MagicBot.SpoilItem", b =>
                {
                    b.Property<long>("SpoilItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("cardUrl");

                    b.Property<DateTime>("date");

                    b.Property<string>("folder");

                    b.Property<string>("message");

                    b.Property<string>("status");

                    b.HasKey("SpoilItemId");

                    b.ToTable("Spoils");
                });
#pragma warning restore 612, 618
        }
    }
}
