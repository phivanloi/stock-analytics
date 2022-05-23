using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Data
{
    public class VndStockScoreData : BaseData, IVndStockScoreData
    {
        public VndStockScoreData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<List<VndStockScore>> FindAllAsync(string symbol)
        {
            string query = "SELECT * FROM VndStockScores WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<VndStockScore>(query, new { symbol })).AsList();
        }

        public virtual async Task<VndStockScore> FindAsync(string symbol, string criteriaCode)
        {
            string query = "SELECT TOP(1) * FROM VndStockScores WHERE Symbol = @symbol AND CriteriaCode = @criteriaCode";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<VndStockScore>(query, new { symbol, criteriaCode });
        }

        public virtual async Task<bool> SaveVndStockScoreAsync(VndStockScore vndStockScore)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                            DECLARE @checkId AS NVARCHAR(22);
                            SET @checkId = (SELECT TOP(1) Id FROM VndStockScores WHERE Symbol = @Symbol AND CriteriaCode = @CriteriaCode);
                            IF @checkId IS NULL
                            BEGIN
                                INSERT INTO VndStockScores
                                    (Id,
                                    Symbol,
                                    Type,
                                    FiscalDate,
                                    ModelCode,
                                    CriteriaCode,
                                    CriteriaType,
                                    CriteriaName,
                                    Point,
                                    Locale,
                                    CreatedTime,
                                    UpdatedTime)
                                VALUES
                                    (@Id,
                                    @Symbol,
                                    @Type,
                                    @FiscalDate,
                                    @ModelCode,
                                    @CriteriaCode,
                                    @CriteriaType,
                                    @CriteriaName,
                                    @Point,
                                    @Locale,
                                    GETDATE(),
                                    GETDATE());
                            END
                            ELSE
                            BEGIN
                                UPDATE VndStockScores SET
                                    Type = @Type,
                                    FiscalDate = @FiscalDate,
                                    ModelCode = @ModelCode,
                                    CriteriaType = @CriteriaType,
                                    CriteriaName = @CriteriaName,
                                    Point = @Point,
                                    Locale = @Locale,
                                    UpdatedTime = GETDATE()
                                WHERE
                                    Id = @checkId
                            END
                            COMMIT";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, vndStockScore) > 0;
        }
    }
}