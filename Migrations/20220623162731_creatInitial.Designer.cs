﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Task.UI.Data;

#nullable disable

namespace Task.UI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220623162731_creatInitial")]
    partial class creatInitial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Task.UI.Data.CiscoPSSProducts", b =>
                {
                    b.Property<int>("Band")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Band"), 1L, 1);

                    b.Property<string>("CategoryCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("DiscountPrice")
                        .HasColumnType("float");

                    b.Property<string>("ItemDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("ListPrice")
                        .HasColumnType("float");

                    b.Property<string>("Manufacturer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("MinDiscount")
                        .HasColumnType("float");

                    b.Property<string>("PartSKU")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Band");

                    b.HasIndex("PartSKU")
                        .IsUnique();

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Task.UI.Data.CiscoPSSServices", b =>
                {
                    b.Property<int>("Band")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Band"), 1L, 1);

                    b.Property<string>("CategoryCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("DiscountPrice")
                        .HasColumnType("float");

                    b.Property<string>("ItemDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("ListPrice")
                        .HasColumnType("float");

                    b.Property<string>("Manufacturer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("MinDiscount")
                        .HasColumnType("float");

                    b.Property<string>("PartSKU")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Band");

                    b.HasIndex("PartSKU")
                        .IsUnique();

                    b.ToTable("Services");
                });

            modelBuilder.Entity("Task.UI.Data.Citrix3PPSS", b =>
                {
                    b.Property<int>("Band")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Band"), 1L, 1);

                    b.Property<string>("CategoryCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("DiscountPrice")
                        .HasColumnType("float");

                    b.Property<string>("ItemDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("ListPrice")
                        .HasColumnType("float");

                    b.Property<string>("Manufacturer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("MinDiscount")
                        .HasColumnType("float");

                    b.Property<string>("PartSKU")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Band");

                    b.HasIndex("PartSKU")
                        .IsUnique();

                    b.ToTable("Citrix");
                });
#pragma warning restore 612, 618
        }
    }
}
