// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pl.Sas.Scheduler.DataContexts;

#nullable disable

namespace Pl.Sas.Scheduler.Migrations.AnalyticsDb
{
    [DbContext(typeof(AnalyticsDbContext))]
    [Migration("20220529152009_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Pl.Sas.Core.Entities.AnalyticsResult", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(22)
                        .HasColumnType("nvarchar(22)");

                    b.Property<byte[]>("CompanyGrowthNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("CompanyGrowthScore")
                        .HasColumnType("int");

                    b.Property<byte[]>("CompanyValueNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("CompanyValueScore")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("FiinNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("FiinScore")
                        .HasColumnType("int");

                    b.Property<byte[]>("MarketNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("MarketScore")
                        .HasColumnType("int");

                    b.Property<byte[]>("StockNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("StockScore")
                        .HasColumnType("int");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)");

                    b.Property<float>("TargetPrice")
                        .HasColumnType("real");

                    b.Property<byte[]>("TargetPriceNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("VndNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("VndScore")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Symbol");

                    b.ToTable("AnalyticsResults");
                });

            modelBuilder.Entity("Pl.Sas.Core.Entities.KeyValue", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(22)
                        .HasColumnType("nvarchar(22)");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.ToTable("KeyValues");
                });

            modelBuilder.Entity("Pl.Sas.Core.Entities.TradingResult", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(22)
                        .HasColumnType("nvarchar(22)");

                    b.Property<string>("AssetPosition")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<float>("BuyPrice")
                        .HasColumnType("real");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<float>("FixedCapital")
                        .HasColumnType("real");

                    b.Property<bool>("IsBuy")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSell")
                        .HasColumnType("bit");

                    b.Property<int>("LoseNumber")
                        .HasColumnType("int");

                    b.Property<int>("Principle")
                        .HasColumnType("int");

                    b.Property<float>("Profit")
                        .HasColumnType("real");

                    b.Property<float>("SellPrice")
                        .HasColumnType("real");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)");

                    b.Property<float>("TotalTax")
                        .HasColumnType("real");

                    b.Property<byte[]>("TradingNotes")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("WinNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Symbol", "Principle");

                    b.ToTable("TradingResults");
                });
#pragma warning restore 612, 618
        }
    }
}
