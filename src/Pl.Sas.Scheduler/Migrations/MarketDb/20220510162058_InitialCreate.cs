using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pl.Sas.Scheduler.Migrations.MarketDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SubsectorCode = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    IndustryName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Supersector = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Sector = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Subsector = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FoundingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ListingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CharterCapital = table.Column<float>(type: "real", nullable: false),
                    NumberOfEmployee = table.Column<int>(type: "int", nullable: false),
                    BankNumberOfBranch = table.Column<int>(type: "int", nullable: false),
                    CompanyProfile = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstPrice = table.Column<float>(type: "real", nullable: false),
                    IssueShare = table.Column<float>(type: "real", nullable: false),
                    ListedValue = table.Column<float>(type: "real", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MarketCap = table.Column<float>(type: "real", nullable: false),
                    SharesOutStanding = table.Column<float>(type: "real", nullable: false),
                    Bv = table.Column<float>(type: "real", nullable: false),
                    Beta = table.Column<float>(type: "real", nullable: false),
                    Eps = table.Column<float>(type: "real", nullable: false),
                    DilutedEps = table.Column<float>(type: "real", nullable: false),
                    Pe = table.Column<float>(type: "real", nullable: false),
                    Pb = table.Column<float>(type: "real", nullable: false),
                    DividendYield = table.Column<float>(type: "real", nullable: false),
                    TotalRevenue = table.Column<float>(type: "real", nullable: false),
                    Profit = table.Column<float>(type: "real", nullable: false),
                    Asset = table.Column<float>(type: "real", nullable: false),
                    Roe = table.Column<float>(type: "real", nullable: false),
                    Roa = table.Column<float>(type: "real", nullable: false),
                    Npl = table.Column<float>(type: "real", nullable: false),
                    FinanciallEverage = table.Column<float>(type: "real", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorporateActions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExrightDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PublicDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    EventListCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Value = table.Column<float>(type: "real", nullable: false),
                    Ratio = table.Column<float>(type: "real", nullable: false),
                    Description = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EventCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiinEvaluates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    IcbRank = table.Column<int>(type: "int", nullable: false),
                    IcbTotalRanked = table.Column<int>(type: "int", nullable: false),
                    IndexRank = table.Column<int>(type: "int", nullable: false),
                    IndexTotalRanked = table.Column<int>(type: "int", nullable: false),
                    IcbCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComGroupCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Growth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Momentum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vgm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControlStatusCode = table.Column<int>(type: "int", nullable: false),
                    ControlStatusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiinEvaluates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialGrowths",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Asset = table.Column<float>(type: "real", nullable: false),
                    ValuePershare = table.Column<float>(type: "real", nullable: false),
                    OwnerCapital = table.Column<float>(type: "real", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialGrowths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialIndicators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    YearReport = table.Column<int>(type: "int", nullable: false),
                    LengthReport = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<float>(type: "real", nullable: false),
                    Profit = table.Column<float>(type: "real", nullable: false),
                    Eps = table.Column<float>(type: "real", nullable: false),
                    DilutedEps = table.Column<float>(type: "real", nullable: false),
                    Pe = table.Column<float>(type: "real", nullable: false),
                    DilutedPe = table.Column<float>(type: "real", nullable: false),
                    Roe = table.Column<float>(type: "real", nullable: false),
                    Roa = table.Column<float>(type: "real", nullable: false),
                    Roic = table.Column<float>(type: "real", nullable: false),
                    GrossProfitMargin = table.Column<float>(type: "real", nullable: false),
                    NetProfitMargin = table.Column<float>(type: "real", nullable: false),
                    DebtEquity = table.Column<float>(type: "real", nullable: false),
                    DebtAsset = table.Column<float>(type: "real", nullable: false),
                    QuickRatio = table.Column<float>(type: "real", nullable: false),
                    CurrentRatio = table.Column<float>(type: "real", nullable: false),
                    Pb = table.Column<float>(type: "real", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialIndicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Industries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leaderships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PositionLevel = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockPrices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TradingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PriceChange = table.Column<float>(type: "real", nullable: false),
                    PerPriceChange = table.Column<float>(type: "real", nullable: false),
                    CeilingPrice = table.Column<float>(type: "real", nullable: false),
                    FloorPrice = table.Column<float>(type: "real", nullable: false),
                    RefPrice = table.Column<float>(type: "real", nullable: false),
                    OpenPrice = table.Column<float>(type: "real", nullable: false),
                    HighestPrice = table.Column<float>(type: "real", nullable: false),
                    LowestPrice = table.Column<float>(type: "real", nullable: false),
                    ClosePrice = table.Column<float>(type: "real", nullable: false),
                    AveragePrice = table.Column<float>(type: "real", nullable: false),
                    ClosePriceAdjusted = table.Column<float>(type: "real", nullable: false),
                    TotalMatchVol = table.Column<float>(type: "real", nullable: false),
                    TotalMatchVal = table.Column<float>(type: "real", nullable: false),
                    TotalDealVal = table.Column<float>(type: "real", nullable: false),
                    TotalDealVol = table.Column<float>(type: "real", nullable: false),
                    ForeignBuyVolTotal = table.Column<float>(type: "real", nullable: false),
                    ForeignCurrentRoom = table.Column<float>(type: "real", nullable: false),
                    ForeignSellVolTotal = table.Column<float>(type: "real", nullable: false),
                    ForeignBuyValTotal = table.Column<float>(type: "real", nullable: false),
                    ForeignSellValTotal = table.Column<float>(type: "real", nullable: false),
                    TotalBuyTrade = table.Column<float>(type: "real", nullable: false),
                    TotalBuyTradeVol = table.Column<float>(type: "real", nullable: false),
                    TotalSellTrade = table.Column<float>(type: "real", nullable: false),
                    TotalSellTradeVol = table.Column<float>(type: "real", nullable: false),
                    NetBuySellVol = table.Column<float>(type: "real", nullable: false),
                    NetBuySellVal = table.Column<float>(type: "real", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockRecommendations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Firm = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Analyst = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReportPrice = table.Column<float>(type: "real", nullable: false),
                    TargetPrice = table.Column<float>(type: "real", nullable: false),
                    AvgTargetPrice = table.Column<float>(type: "real", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockRecommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Exchange = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TradingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ZipDetails = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VndStockScores",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    FiscalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModelCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CriteriaCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CriteriaType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CriteriaName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Point = table.Column<float>(type: "real", nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VndStockScores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Symbol_SubsectorCode",
                table: "Companies",
                columns: new[] { "Symbol", "SubsectorCode" });

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_EventCode",
                table: "CorporateActions",
                column: "EventCode");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_EventCode_Symbol_Exchange",
                table: "CorporateActions",
                columns: new[] { "EventCode", "Symbol", "Exchange" });

            migrationBuilder.CreateIndex(
                name: "IX_FiinEvaluates_Symbol",
                table: "FiinEvaluates",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialGrowths_Symbol",
                table: "FinancialGrowths",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialIndicators_Symbol",
                table: "FinancialIndicators",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Code",
                table: "Industries",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderships_Symbol",
                table: "Leaderships",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_StockPrices_Symbol",
                table: "StockPrices",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_StockPrices_Symbol_TradingDate",
                table: "StockPrices",
                columns: new[] { "Symbol", "TradingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockPrices_TradingDate",
                table: "StockPrices",
                column: "TradingDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockRecommendations_Symbol_Analyst_ReportDate",
                table: "StockRecommendations",
                columns: new[] { "Symbol", "Analyst", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Symbol_Type",
                table: "Stocks",
                columns: new[] { "Symbol", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Symbol_TradingDate",
                table: "StockTransactions",
                columns: new[] { "Symbol", "TradingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VndStockScores_Symbol",
                table: "VndStockScores",
                column: "Symbol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "CorporateActions");

            migrationBuilder.DropTable(
                name: "FiinEvaluates");

            migrationBuilder.DropTable(
                name: "FinancialGrowths");

            migrationBuilder.DropTable(
                name: "FinancialIndicators");

            migrationBuilder.DropTable(
                name: "Industries");

            migrationBuilder.DropTable(
                name: "Leaderships");

            migrationBuilder.DropTable(
                name: "StockPrices");

            migrationBuilder.DropTable(
                name: "StockRecommendations");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "VndStockScores");
        }
    }
}
