using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;
using Pl.Sas.Core;

namespace Pl.Sas.Infrastructure.Data
{
    public class CorporateActionData : BaseData, ICorporateActionData
    {
        private readonly ILogger<ScheduleData> _logger;

        public CorporateActionData(ILogger<ScheduleData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task<List<CorporateAction>> FindAllAsync(string symbol, string[] eventCodes, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = "SELECT * FROM CorporateActions WHERE Symbol IS NOT NULL ";
            if (!string.IsNullOrEmpty(symbol))
            {
                query += " AND Symbol = @symbol ";
            }
            if (eventCodes?.Length > 0)
            {
                query += " AND EventCode IN @eventCodes ";
            }
            if (fromDate.HasValue)
            {
                query += " AND PublicDate >= @fromDate ";
            }
            if (toDate.HasValue)
            {
                query += " AND PublicDate <= @toDate ";
            }
            query += " ORDER BY PublicDate DESC";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<CorporateAction>(query, new { symbol, eventCodes, toDate, fromDate })).AsList();
        }

        public virtual async Task<List<CorporateAction>> FindAllForViewPageAsync(string symbol, string[] eventCodes, string exchange)
        {
            var query = "SELECT * FROM CorporateActions WHERE ExrightDate >= @date ";
            if (!string.IsNullOrEmpty(symbol))
            {
                query += " AND Symbol = @symbol ";
            }
            if (eventCodes?.Length > 0)
            {
                query += " AND EventCode IN @eventCodes ";
            }
            if (!string.IsNullOrEmpty(exchange))
            {
                query += " AND Exchange = @exchange ";
            }
            query += " ORDER BY ExrightDate";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<CorporateAction>(query, new { symbol, eventCodes, exchange, date = DateTime.Now.Date })).AsList();
        }

        public virtual async Task<IReadOnlyList<CorporateAction>> GetCorporateActionsForCheckDownloadAsync(string symbol)
        {
            var query = "SELECT Symbol, EventCode, ExrightDate FROM CorporateActions WHERE Symbol = @symbol ";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<CorporateAction>(query, new { symbol })).AsList();
        }

        public virtual async Task<List<CorporateAction>> GetTradingByExrightDateAsync()
        {
            var tradingDate = DateTime.Now.Date;
            var query = "SELECT * FROM CorporateActions WHERE CONVERT(date, ExrightDate) = @tradingDate AND EventCode IN ('DIV','ISS')";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<CorporateAction>(query, new { tradingDate })).AsList();
        }

        public virtual async Task BulkInserAsync(IEnumerable<CorporateAction> corporateActions)
        {
            if (corporateActions is null || !corporateActions.Any())
            {
                return;
            }

            var dataTableTemp = corporateActions.ToDataTable();
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
                    sqlBulkCopy.DestinationTableName = "CorporateActions";
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

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE CorporateActions WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }
    }
}