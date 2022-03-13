﻿// <auto-generated />
using System;
using NaGet.Database.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NaGet.Database.MySql.Migrations
{
    [DbContext(typeof(MySqlContext))]
    [Migration("20200109085155_AddReleaseNotesStringColumn")]
    partial class AddReleaseNotesStringColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("NaGet.Core.Package", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Authors")
                        .HasMaxLength(4000);

                    b.Property<string>("Description")
                        .HasMaxLength(4000);

                    b.Property<long>("Downloads");

                    b.Property<bool>("HasReadme");

                    b.Property<string>("IconUrl")
                        .HasMaxLength(4000);

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<bool>("IsPrerelease");

                    b.Property<string>("Language")
                        .HasMaxLength(20);

                    b.Property<string>("LicenseUrl")
                        .HasMaxLength(4000);

                    b.Property<bool>("Listed");

                    b.Property<string>("MinClientVersion")
                        .HasMaxLength(44);

                    b.Property<string>("NormalizedVersionString")
                        .IsRequired()
                        .HasColumnName("Version")
                        .HasMaxLength(64);

                    b.Property<string>("OriginalVersionString")
                        .HasColumnName("OriginalVersion")
                        .HasMaxLength(64);

                    b.Property<string>("ProjectUrl")
                        .HasMaxLength(4000);

                    b.Property<DateTime>("Published");

                    b.Property<string>("ReleaseNotes")
                        .HasColumnName("ReleaseNotes")
                        .HasMaxLength(4000);

                    b.Property<string>("RepositoryType")
                        .HasMaxLength(100);

                    b.Property<string>("RepositoryUrl")
                        .HasMaxLength(4000);

                    b.Property<bool>("RequireLicenseAcceptance");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<int>("SemVerLevel");

                    b.Property<string>("Summary")
                        .HasMaxLength(4000);

                    b.Property<string>("Tags")
                        .HasMaxLength(4000);

                    b.Property<string>("Title")
                        .HasMaxLength(256);

                    b.HasKey("Key");

                    b.HasIndex("Id");

                    b.HasIndex("Id", "NormalizedVersionString")
                        .IsUnique();

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("NaGet.Core.PackageDependency", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Id")
                        .HasMaxLength(128);

                    b.Property<int?>("PackageKey");

                    b.Property<string>("TargetFramework")
                        .HasMaxLength(256);

                    b.Property<string>("VersionRange")
                        .HasMaxLength(256);

                    b.HasKey("Key");

                    b.HasIndex("Id");

                    b.HasIndex("PackageKey");

                    b.ToTable("PackageDependencies");
                });

            modelBuilder.Entity("NaGet.Core.PackageType", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .HasMaxLength(512);

                    b.Property<int>("PackageKey");

                    b.Property<string>("Version")
                        .HasMaxLength(64);

                    b.HasKey("Key");

                    b.HasIndex("Name");

                    b.HasIndex("PackageKey");

                    b.ToTable("PackageTypes");
                });

            modelBuilder.Entity("NaGet.Core.TargetFramework", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Moniker")
                        .HasMaxLength(256);

                    b.Property<int>("PackageKey");

                    b.HasKey("Key");

                    b.HasIndex("Moniker");

                    b.HasIndex("PackageKey");

                    b.ToTable("TargetFrameworks");
                });

            modelBuilder.Entity("NaGet.Core.PackageDependency", b =>
                {
                    b.HasOne("NaGet.Core.Package", "Package")
                        .WithMany("Dependencies")
                        .HasForeignKey("PackageKey");
                });

            modelBuilder.Entity("NaGet.Core.PackageType", b =>
                {
                    b.HasOne("NaGet.Core.Package", "Package")
                        .WithMany("PackageTypes")
                        .HasForeignKey("PackageKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NaGet.Core.TargetFramework", b =>
                {
                    b.HasOne("NaGet.Core.Package", "Package")
                        .WithMany("TargetFrameworks")
                        .HasForeignKey("PackageKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
