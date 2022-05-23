using Dapper;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Microsoft.Data.SqlClient;

namespace Pl.Sas.Infrastructure.Data
{
    public class FiinEvaluatedData : BaseData, IFiinEvaluatedData
    {
        public FiinEvaluatedData(IOptionsMonitor<ConnectionStrings> options) : base(options) { }

        public virtual async Task<IReadOnlyList<FiinEvaluated>> FindAllAsync(string symbol)
        {
            string query = @"SELECT * FROM FiinEvaluates WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<FiinEvaluated>(query, new { symbol })).AsList();
        }

        public virtual async Task<FiinEvaluated> FindAsync(string symbol)
        {
            string query = @"SELECT TOP(1) * FROM FiinEvaluates WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<FiinEvaluated>(query, new { symbol });
        }

        public virtual async Task<bool> SaveFiinEvaluateAsync(FiinEvaluated fiinEvaluated)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                                DECLARE @fiinEvaluateId AS NVARCHAR(22);
                                SET @fiinEvaluateId = (SELECT TOP(1) Id FROM FiinEvaluates WHERE Symbol = @Symbol);
                                IF @fiinEvaluateId IS NULL
                                BEGIN
                                    INSERT INTO FiinEvaluates
                                        (Id,
                                        Symbol,
                                        IcbRank,
                                        IcbTotalRanked,
                                        IndexRank,
                                        IndexTotalRanked,
                                        IcbCode,
                                        ComGroupCode,
                                        Growth,
                                        Value,
                                        Momentum,
                                        Vgm,
                                        ControlStatusCode,
                                        ControlStatusName,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @Symbol,
                                        @IcbRank,
                                        @IcbTotalRanked,
                                        @IndexRank,
                                        @IndexTotalRanked,
                                        @IcbCode,
                                        @ComGroupCode,
                                        @Growth,
                                        @Value,
                                        @Momentum,
                                        @Vgm,
                                        @ControlStatusCode,
                                        @ControlStatusName,
                                        GETDATE(),
                                        GETDATE());
                                END
                                ELSE
                                BEGIN
                                    UPDATE FiinEvaluates SET
                                        IcbRank = @IcbRank,
                                        IcbTotalRanked = @IcbTotalRanked,
                                        IndexRank = @IndexRank,
                                        IndexTotalRanked = @IndexTotalRanked,
                                        IcbCode = @IcbCode,
                                        ComGroupCode = @ComGroupCode,
                                        Growth = @Growth,
                                        Value = @Value,
                                        Momentum = @Momentum,
                                        Vgm = @Vgm,
                                        ControlStatusCode = @ControlStatusCode,
                                        ControlStatusName = @ControlStatusName,
                                        UpdatedTime = GETDATE()
                                    WHERE
                                        Id = @fiinEvaluateId
                                END
                            COMMIT";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, fiinEvaluated) > 0;
        }
    }
}