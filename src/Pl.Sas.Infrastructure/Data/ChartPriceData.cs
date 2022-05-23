﻿using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Data;

namespace Pl.Sas.Infrastructure.Data
{
    public class ChartPriceData : BaseData, IChartPriceData
    {
        private readonly ILogger<ChartPriceData> _logger;

        public ChartPriceData(ILogger<ChartPriceData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task<List<ChartPrice>> FindAllAsync(string symbol, string type = "D", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = "SELECT * FROM ChartPrices WHERE Symbol = @symbol AND Type = @type ";
            if (fromDate.HasValue)
            {
                query += " AND TradingDate >= @fromDate ";
            }

            if (toDate.HasValue)
            {
                query += " AND TradingDate <= @toDate ";
            }

            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<ChartPrice>(query, new { type, symbol, fromDate, toDate })).AsList();
        }

        public virtual async Task<bool> ResetChartPriceAsync(List<ChartPrice> chartPrices, string symbol, string type = "D")
        {
            if (chartPrices.Count > 0)
            {
                var checkDelete = await DeleteAsync(symbol, type);
                if (checkDelete)
                {
                    await BulkInserAsync(chartPrices);
                    return true;
                }
            }
            return false;
        }

        public virtual async Task BulkInserAsync(IEnumerable<ChartPrice> chartPrices)
        {
            if (chartPrices is null || !chartPrices.Any())
            {
                return;
            }

            var dataTableTemp = chartPrices.ToDataTable();
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                await _dbAsyncRetry.ExecuteAsync(async () =>
                {
                    using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran))
                    {
                        foreach (DataColumn column in dataTableTemp.Columns)
                        {
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                        }

                        sqlBulkCopy.BulkCopyTimeout = 300;
                        sqlBulkCopy.DestinationTableName = "ChartPrices";
                        await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                    }
                    tran.Commit();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChartPrice BulkInserAsync error");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<bool> UpsertAsync(ChartPrice chartPrice)
        {
            var query = $@" SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
                            BEGIN TRAN
                                DECLARE @currentId AS NVARCHAR(22);
                                SET @currentId = (SELECT TOP(1) Id FROM ChartPrices WHERE Symbol = @Symbol AND TradingDate = @TradingDate AND [Type] = @Type);
                                IF @currentId IS NULL
                                BEGIN
                                    INSERT INTO ChartPrices
                                        (Id,
                                        Symbol,
                                        TradingDate,
                                        Type,
                                        OpenPrice,
                                        HighestPrice,
                                        LowestPrice,
                                        ClosePrice,
                                        TotalMatchVol,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @Symbol,
                                        @TradingDate,
                                        @Type,
                                        @OpenPrice,
                                        @HighestPrice,
                                        @LowestPrice,
                                        @ClosePrice,
                                        @TotalMatchVol,
                                        GETDATE(),
                                        GETDATE());
                                END
                                ELSE
                                BEGIN
                                    UPDATE ChartPrices SET
                                        OpenPrice = @OpenPrice
                                        HighestPrice = @HighestPrice
                                        LowestPrice = @LowestPrice
                                        ClosePrice = @ClosePrice
                                        TotalMatchVol = @TotalMatchVol
                                        UpdatedTime = GETDATE()
                                    WHERE
                                        Id = @currentId
                                END
                            COMMIT";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, chartPrice) > 0;
        }

        public virtual async Task<bool> DeleteAsync(string symbol, string type)
        {
            var query = "DELETE ChartPrices WHERE Symbol = @symbol AND [Type] = @type";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { symbol, type }) > 0;
        }
    }
}