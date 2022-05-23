using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Data
{
    public class StockRecommendationData : BaseData, IStockRecommendationData
    {
        public StockRecommendationData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<List<StockRecommendation>> FindAllAsync(string symbol, DateTime? startDate)
        {
            string query = @"SELECT * FROM StockRecommendations WHERE Symbol = @symbol";
            if (startDate is not null)
            {
                query += " AND ReportDate >= @startDate";
            }
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockRecommendation>(query, new { symbol, startDate })).AsList();
        }

        public virtual async Task<List<StockRecommendation>> GetTopReportInSixMonthAsync(string symbol, int top = 5)
        {
            var endDate = DateTime.Now.AddMonths(-6);
            string query = @"SELECT TOP(@top) * FROM StockRecommendations WHERE Symbol = @symbol AND ReportDate >= @endDate ORDER BY ReportDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockRecommendation>(query, new { symbol, top, endDate })).AsList();
        }

        public virtual async Task<bool> SaveStockRecommendationAsync(StockRecommendation vndSignal)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                            DECLARE @checkId AS NVARCHAR(22);
                            SET @checkId = (SELECT TOP(1) Id FROM StockRecommendations WHERE Symbol = @Symbol AND ReportDate = @ReportDate AND Analyst = @Analyst);
                            IF @checkId IS NULL
                            BEGIN
                                INSERT INTO StockRecommendations
                                    (Id,
                                    Symbol,
                                    Firm,
                                    [Type],
                                    ReportDate,
                                    Source,
                                    Analyst,
                                    ReportPrice,
                                    TargetPrice,
                                    AvgTargetPrice,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@Id,
                                    @Symbol,
                                    @Firm,
                                    @Type,
                                    @ReportDate,
                                    @Source,
                                    @Analyst,
                                    @ReportPrice,
                                    @TargetPrice,
                                    @AvgTargetPrice,
                                    GETDATE(),
                                    GETDATE());
                            END
                            ELSE
                            BEGIN
                                UPDATE StockRecommendations SET
                                    Firm = @Firm,
                                    Type = @Type,
                                    Source = @Source,
                                    ReportPrice = @ReportPrice,
                                    TargetPrice = @TargetPrice,
                                    AvgTargetPrice = @AvgTargetPrice,
                                    UpdatedTime = GETDATE()
                                WHERE
                                    Id = @checkId
                            END
                            COMMIT";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, vndSignal) > 0;
        }
    }
}