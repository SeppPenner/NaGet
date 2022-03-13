// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NaGet.Database.MySql.Migrations
{
    [DbContext(typeof(MySqlContext))]
    [Migration("20181212113156_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("NaGet.Core.Entities.Package", b =>
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

                    b.Property<string>("Language")
                        .HasMaxLength(20);

                    b.Property<string>("LicenseUrl")
                        .HasMaxLength(4000);

                    b.Property<bool>("Listed");

                    b.Property<string>("MinClientVersion")
                        .HasMaxLength(44);

                    b.Property<string>("ProjectUrl")
                        .HasMaxLength(4000);

                    b.Property<DateTime>("Published");

                    b.Property<string>("RepositoryType")
                        .HasMaxLength(100);

                    b.Property<string>("RepositoryUrl")
                        .HasMaxLength(4000);

                    b.Property<bool>("RequireLicenseAcceptance");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("Summary")
                        .HasMaxLength(4000);

                    b.Property<string>("Tags")
                        .HasMaxLength(4000);

                    b.Property<string>("Title")
                        .HasMaxLength(256);

                    b.Property<string>("VersionString")
                        .IsRequired()
                        .HasColumnName("Version")
                        .HasMaxLength(64);

                    b.HasKey("Key");

                    b.HasIndex("Id");

                    b.HasIndex("Id", "VersionString")
                        .IsUnique();

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("NaGet.Core.Entities.PackageDependency", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Id")
                        .HasMaxLength(128);

                    b.Property<int>("PackageKey");

                    b.Property<string>("TargetFramework")
                        .HasMaxLength(256);

                    b.Property<string>("VersionRange")
                        .HasMaxLength(256);

                    b.HasKey("Key");

                    b.HasIndex("PackageKey");

                    b.ToTable("PackageDependencies");
                });

            modelBuilder.Entity("NaGet.Core.Entities.PackageDependency", b =>
                {
                    b.HasOne("NaGet.Core.Entities.Package", "Package")
                        .WithMany("Dependencies")
                        .HasForeignKey("PackageKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
