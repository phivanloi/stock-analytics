using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pl.Sas.Scheduler.Migrations.AnalyticsDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TradingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MarketScore = table.Column<int>(type: "int", nullable: false),
                    MarketNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CompanyValueScore = table.Column<int>(type: "int", nullable: false),
                    CompanyValueNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CompanyGrowthScore = table.Column<int>(type: "int", nullable: false),
                    CompanyGrowthNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    StockScore = table.Column<int>(type: "int", nullable: false),
                    StockNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FiinScore = table.Column<int>(type: "int", nullable: false),
                    FiinNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    VndScore = table.Column<int>(type: "int", nullable: false),
                    VndNote = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TargetPrice = table.Column<float>(type: "real", nullable: false),
                    TargetPriceNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTrackings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    DownloadStatus = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    DownloadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataStatus = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    DataDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnalyticsStatus = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    AnalyticsDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTrackings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Principle = table.Column<int>(type: "int", nullable: false),
                    TradingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBuy = table.Column<bool>(type: "bit", nullable: false),
                    BuyPrice = table.Column<float>(type: "real", nullable: false),
                    IsSell = table.Column<bool>(type: "bit", nullable: false),
                    SellPrice = table.Column<float>(type: "real", nullable: false),
                    Capital = table.Column<float>(type: "real", nullable: false),
                    Profit = table.Column<float>(type: "real", nullable: false),
                    TotalTax = table.Column<float>(type: "real", nullable: false),
                    TradingNotes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsResults_Symbol_TradingDate",
                table: "AnalyticsResults",
                columns: new[] { "Symbol", "TradingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTrackings_Symbol",
                table: "StockTrackings",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_TradingResults_Symbol_TradingDate_Principle",
                table: "TradingResults",
                columns: new[] { "Symbol", "TradingDate", "Principle" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsResults");

            migrationBuilder.DropTable(
                name: "StockTrackings");

            migrationBuilder.DropTable(
                name: "TradingResults");
        }
    }
}
