using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Data;

namespace Pl.Sas.Infrastructure.Data
{
    public class StockPriceData : BaseData, IStockPriceData
    {
        private readonly ILogger<ScheduleData> _logger;

        public StockPriceData(ILogger<ScheduleData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task BulkInserAsync(List<StockPrice> stockPrices)
        {
            if (stockPrices is null || stockPrices.Count <= 0)
            {
                return;
            }

            var dataTableTemp = stockPrices.ToDataTable();
            await _dbAsyncRetry.ExecuteAsync(async () =>
            {
                using SqlConnection connection = new(_connectionStrings.MarketConnection);
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
                        sqlBulkCopy.DestinationTableName = "StockPrices";
                        await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Stock data error => BulkInserStockPricesAsync");
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            });
        }

        public virtual async Task<StockPrice> GetByDayAsync(string symbol, DateTime tradingDate)
        {
            var query = "SELECT TOP(1) * FROM StockPrices WHERE Symbol = @symbol AND TradingDate = @tradingDate";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<StockPrice>(query, new { symbol, tradingDate });
        }

        public virtual async Task<bool> IsExistByDayAsync(string symbol, DateTime tradingDate)
        {
            var query = "SELECT TOP(1) Id FROM StockPrices WHERE Symbol = @symbol AND TradingDate = @tradingDate";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return !string.IsNullOrEmpty(await connection.QueryFirstOrDefaultAsync<string>(query, new { symbol, tradingDate }));
        }

        public virtual async Task<StockPrice> GetLastAsync(string symbol)
        {
            var query = "SELECT TOP(1) * FROM StockPrices WHERE Symbol = @symbol ORDER BY TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<StockPrice>(query, new { symbol });
        }

        public virtual async Task<List<StockPrice>> FindAllAsync(string symbol, int numberItem = 10000, DateTime? fromTradingTime = null, DateTime? toTradingTime = null)
        {
            var whereQuery = " WHERE Symbol = @symbol ";

            if (fromTradingTime.HasValue)
            {
                whereQuery += " AND TradingDate >= @fromTradingTime ";
            }

            if (toTradingTime.HasValue)
            {
                whereQuery += " AND TradingDate <= @toTradingTime ";
            }

            var query = $"SELECT TOP(@numberItem) * FROM StockPrices {whereQuery} ORDER BY TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockPrice>(query, new { symbol, fromTradingTime, numberItem, toTradingTime })).AsList();
        }

        public virtual async Task<List<StockPrice>> FindAllForTradingAsync(string symbol, int numberItem = 10000, DateTime? fromTradingTime = null, DateTime? toTradingTime = null)
        {
            var whereQuery = " WHERE Symbol = @symbol ";

            if (fromTradingTime.HasValue)
            {
                whereQuery += " AND TradingDate >= @fromTradingTime ";
            }

            if (toTradingTime.HasValue)
            {
                whereQuery += " AND TradingDate <= @toTradingTime ";
            }

            var query = @$" SELECT TOP(@numberItem)
                                Symbol,
                                TradingDate,
                                OpenPrice,
                                HighestPrice,
                                LowestPrice,
                                ClosePrice,
                                TotalMatchVol,
                                ClosePriceAdjusted
                            FROM
                                StockPrices 
                            {whereQuery} 
                            ORDER BY
                                TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockPrice>(query, new { symbol, fromTradingTime, numberItem, toTradingTime })).AsList();
        }

        public virtual async Task<List<StockPrice>> GetForDetailPageAsync(string symbol, int numberItem = 10000)
        {
            var query = @$" SELECT TOP(@numberItem)
                                Symbol,
                                TradingDate,
                                ClosePrice,
                                ClosePriceAdjusted,
                                OpenPrice,
                                FloorPrice,
                                CeilingPrice,
                                HighestPrice,
                                LowestPrice,
                                TotalDealVol,
                                TotalMatchVol,
                                ForeignBuyVolTotal,
                                ForeignSellVolTotal,
                                TotalBuyTrade,
                                TotalSellTrade
                            FROM
                                StockPrices
                            WHERE
                                Symbol = @symbol
                            ORDER BY
                                TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockPrice>(query, new { symbol, numberItem })).AsList();
        }

        public virtual async Task<List<StockPrice>> GetForStockViewAsync(string symbol, int numberItem = 10000)
        {
            var query = @$" SELECT TOP(@numberItem)
                                Symbol,
                                TradingDate,
                                ClosePrice,
                                ClosePriceAdjusted,
                                HighestPrice,
                                LowestPrice,
                                TotalMatchVol,
                                ForeignBuyVolTotal,
                                ForeignSellVolTotal,
                                TotalBuyTrade,
                                TotalSellTrade
                            FROM
                                StockPrices
                            WHERE
                                Symbol = @symbol
                            ORDER BY
                                TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockPrice>(query, new { symbol, numberItem })).AsList();
        }

        public virtual async Task<List<StockPrice>> GetForIndustryTrendAnalyticsAsync(string symbol, int top)
        {
            var query = $"SELECT TOP(@top) TradingDate, ClosePrice, ClosePriceAdjusted, TotalMatchVal FROM StockPrices WHERE Symbol = @symbol ORDER BY TradingDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<StockPrice>(query, new { symbol, top })).AsList();
        }

        public virtual async Task<bool> UpdateAsync(StockPrice stockPrice)
        {
            string query = @"    UPDATE StockPrices SET
			                            TradingDate = @TradingDate,
			                            PriceChange = @PriceChange,
			                            PerPriceChange = @PerPriceChange,
			                            CeilingPrice = @CeilingPrice,
			                            FloorPrice = @FloorPrice,
		                                RefPrice = @RefPrice,
		                                OpenPrice = @OpenPrice,
			                            HighestPrice = @HighestPrice,
                                        LowestPrice = @LowestPrice,
                                        ClosePrice = @ClosePrice,
                                        AveragePrice = @AveragePrice,
                                        ClosePriceAdjusted = @ClosePriceAdjusted,
                                        TotalMatchVol = @TotalMatchVol,
                                        TotalMatchVal = @TotalMatchVal,
                                        TotalDealVal = @TotalDealVal,
                                        TotalDealVol = @TotalDealVol,
                                        ForeignBuyVolTotal = @ForeignBuyVolTotal,
                                        ForeignCurrentRoom = @ForeignCurrentRoom,
                                        ForeignSellVolTotal = @ForeignSellVolTotal,
                                        ForeignBuyValTotal = @ForeignBuyValTotal,
                                        ForeignSellValTotal = @ForeignSellValTotal,
                                        TotalBuyTrade = @TotalBuyTrade,
                                        TotalBuyTradeVol = @TotalBuyTradeVol,
                                        TotalSellTrade = @TotalSellTrade,
                                        TotalSellTradeVol = @TotalSellTradeVol,
                                        NetBuySellVol = @NetBuySellVol,
                                        NetBuySellVal = @NetBuySellVal,
                                        UpdatedTime = GETDATE()
		                            WHERE
			                            Id = @Id";
            return await _dbAsyncRetry.ExecuteAsync(async () =>
            {
                using SqlConnection connection = new(_connectionStrings.MarketConnection);
                return await connection.ExecuteAsync(query, stockPrice) > 0;
            });
        }

        public virtual async Task<bool> InsertAsync(StockPrice stockPrice)
        {
            var query = $@" INSERT INTO StockPrices
                                (Id,
                                Symbol,
                                TradingDate,
                                PriceChange,
                                PerPriceChange,
                                CeilingPrice,
                                FloorPrice,
                                RefPrice,
                                OpenPrice,
                                HighestPrice,
                                LowestPrice,
                                ClosePrice,
                                AveragePrice,
                                ClosePriceAdjusted,
                                TotalMatchVol,
                                TotalMatchVal,
                                TotalDealVal,
                                TotalDealVol,
                                ForeignBuyVolTotal,
                                ForeignCurrentRoom,
                                ForeignSellVolTotal,
                                ForeignBuyValTotal,
                                ForeignSellValTotal,
                                TotalBuyTrade,
                                TotalBuyTradeVol,
                                TotalSellTrade,
                                TotalSellTradeVol,
                                NetBuySellVol,
                                NetBuySellVal,
                                CreatedTime,
                                UpdatedTime)
                            VALUES
                                (@Id,
                                @Symbol,
                                @TradingDate,
                                @PriceChange,
                                @PerPriceChange,
                                @CeilingPrice,
                                @FloorPrice,
                                @RefPrice,
                                @OpenPrice,
                                @HighestPrice,
                                @LowestPrice,
                                @ClosePrice,
                                @AveragePrice,
                                @ClosePriceAdjusted,
                                @TotalMatchVol,
                                @TotalMatchVal,
                                @TotalDealVal,
                                @TotalDealVol,
                                @ForeignBuyVolTotal,
                                @ForeignCurrentRoom,
                                @ForeignSellVolTotal,
                                @ForeignBuyValTotal,
                                @ForeignSellValTotal,
                                @TotalBuyTrade,
                                @TotalBuyTradeVol,
                                @TotalSellTrade,
                                @TotalSellTradeVol,
                                @NetBuySellVol,
                                @NetBuySellVal,
                                GETDATE(),
                                GETDATE())";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, stockPrice) > 0;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE StockPrices WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<bool> DeleteBySymbolAsync(string symbol)
        {
            var query = "DELETE StockPrices WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { symbol }) > 0;
        }
    }
}