using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class StockFeatureData : IStockFeatureData
    {
        private readonly Connections _connections;
        private readonly ILogger<StockFeatureData> _logger;

        public StockFeatureData(
            ILogger<StockFeatureData> logger,
            IOptions<Connections> options)
        {
            _connections = options.Value;
            _logger = logger;
        }

        public virtual async Task<StockFeature> FindAsync(string symbol)
        {
            string query = @"SELECT TOP(1) * FROM StockFeatures WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.QueryFirstOrDefaultAsync<StockFeature>(query, new { symbol });
        }

        public virtual async Task<bool> InsertAsync(StockFeature stockFeature)
        {
            var query = $@"   INSERT INTO StockFeatures
                                        (Id,
                                        Symbol,
                                        FirstEmaBuy,
                                        SecondEmaBuy,
                                        FirstEmaSell,
                                        SecondEmaSell,
                                        StochasticBuy,
                                        HighestStochasticBuy,
                                        LowestStochasticBuy,
                                        StochasticSell,
                                        HighestStochasticSell,
                                        LowestStochasticSell,
                                        GoodBuyTimes,
                                        GoodSellTimes,
                                        AutoRank,
                                        Rank,
                                        NumberWinTrander,
                                        NumberLoseTrander,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @Symbol,
                                        @FirstEmaBuy,
                                        @SecondEmaBuy,
                                        @FirstEmaSell,
                                        @SecondEmaSell,
                                        @StochasticBuy,
                                        @HighestStochasticBuy,
                                        @LowestStochasticBuy,
                                        @StochasticSell,
                                        @HighestStochasticSell,
                                        @LowestStochasticSell,
                                        @GoodBuyTimes,
                                        @GoodSellTimes,
                                        @AutoRank,
                                        @Rank,
                                        @NumberWinTrander,
                                        @NumberLoseTrander,
                                        @CreatedTime,
                                        @UpdatedTime)";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, stockFeature) > 0;
        }

        public virtual async Task<bool> SaveEmaOptimalAsync(string symbol, int firstEmaBuy, int secondEmaBuy, int firstEmaSell, int secondEmaSell)
        {
            var query = @"  UPDATE 
                                StockFeatures 
                            SET 
                                FirstEmaBuy = @firstEmaBuy, 
                                SecondEmaBuy = @secondEmaBuy, 
                                FirstEmaSell = @firstEmaSell, 
                                SecondEmaSell = @secondEmaSell, 
                                UpdatedTime = GETDATE() 
                            WHERE 
                                Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, firstEmaBuy, secondEmaBuy, firstEmaSell, secondEmaSell }) > 0;
        }

        public virtual async Task<bool> SaveStochasticOptimalAsync(string symbol, string stochasticBuy, decimal highestStochasticBuy, int lowestStochasticBuy, int stochasticSell, int highestStochasticSell, int lowestStochasticSell)
        {
            var query = @"  UPDATE 
                                StockFeatures 
                            SET 
                                StochasticBuy = @stochasticBuy, 
                                HighestStochasticBuy = @highestStochasticBuy, 
                                LowestStochasticBuy = @lowestStochasticBuy, 
                                StochasticSell = @stochasticSell, 
                                HighestStochasticSell = @highestStochasticSell, 
                                LowestStochasticSell = @lowestStochasticSell, 
                                UpdatedTime = GETDATE() 
                            WHERE 
                                Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, stochasticBuy, highestStochasticBuy, lowestStochasticBuy, stochasticSell, highestStochasticSell, lowestStochasticSell }) > 0;
        }

        public virtual async Task<bool> SaveGoodBuyAndSellTimesAsync(string symbol, string goodBuyTimes, int goodSellTimes)
        {
            var query = "UPDATE StockFeatures SET GoodBuyTimes = @goodBuyTimes, GoodSellTimes = @goodSellTimes, UpdatedTime = GETDATE() WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, goodBuyTimes, goodSellTimes }) > 0;
        }

        public virtual async Task<bool> SaveAutoRankAsync(string symbol, int autoRank)
        {
            var query = @"  UPDATE
                                StockFeatures
                            SET
                                AutoRank = @autoRank,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, autoRank }) > 0;
        }

        public virtual async Task<bool> SaveRankAsync(string symbol, int rank)
        {
            var query = @"  UPDATE
                                StockFeatures
                            SET
                                Rank = @rank,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, new { symbol, rank }) > 0;
        }

        public virtual async Task<bool> SaveTestTradingFindAsync(StockFeature stockFeature)
        {
            var query = @"  UPDATE
                                StockFeatures
                            SET
                                FirstEmaBuy = @firstEmaBuy, 
                                SecondEmaBuy = @secondEmaBuy, 
                                FirstEmaSell = @firstEmaSell, 
                                SecondEmaSell = @secondEmaSell, 
                                StochasticBuy = @stochasticBuy, 
                                HighestStochasticBuy = @highestStochasticBuy, 
                                LowestStochasticBuy = @lowestStochasticBuy, 
                                StochasticSell = @stochasticSell, 
                                HighestStochasticSell = @highestStochasticSell, 
                                LowestStochasticSell = @lowestStochasticSell, 
                                NumberWinTrander = @NumberWinTrander,
                                NumberLoseTrander = @NumberLoseTrander,
                                UpdatedTime = GETDATE()
                            WHERE
                                Symbol = @symbol";
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            return await connection.ExecuteAsync(query, stockFeature) > 0;
        }

        public virtual async Task BulkInserAsync(IEnumerable<StockFeature> StockFeatures)
        {
            if (StockFeatures?.Count() <= 0)
            {
                return;
            }

            var dataTableTemp = StockFeatures.ToDataTable();
            using SqlConnection connection = new(_connections.AnalyticsConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran))
                {
                    foreach (DataColumn column in dataTableTemp.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                    }

                    sqlBulkCopy.BulkCopyTimeout = 300;
                    sqlBulkCopy.DestinationTableName = "StockFeatures";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkInserAsync error");
                tran.Rollback();
                throw;
            }
        }
    }
}