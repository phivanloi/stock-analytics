using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Pl.Sas.Core;

namespace Pl.Sas.Infrastructure.Data
{
    public class IndustryData : BaseData, IIndustryData
    {
        private readonly ILogger<IndustryData> _logger;

        public IndustryData(ILogger<IndustryData> logger, IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _logger = logger;
        }

        public virtual async Task BulkInserAsync(List<Industry> industries)
        {
            if (industries is null || industries.Count <= 0)
            {
                return;
            }

            var dataTableTemp = industries.ToDataTable();

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
                    sqlBulkCopy.DestinationTableName = "Industries";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Industry data error => BulkInserAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<IEnumerable<Industry>> FindAllAsync()
        {
            var query = "SELECT * FROM Industries ORDER BY [Name]";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryAsync<Industry>(query);
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE Industries WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<string> GetIdByNameAsync(string name)
        {
            var query = "SELECT Id FROM Industries WHERE UPPER([Name]) = UPPER(@name)";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<string>(query, new { name });
        }

        public virtual async Task<Industry> GetByCodeAsync(string code)
        {
            var query = "SELECT * FROM Industries WHERE Code = @code";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<Industry>(query, new { code });
        }

        public virtual async Task<Industry> GetByIdAsync(string id)
        {
            var query = "SELECT * FROM Industries WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<Industry>(query, new { id });
        }

        public virtual async Task<bool> InsertAsync(Industry industry)
        {
            var query = $@"   INSERT INTO Industries
                                        (Id,
                                        Code,
                                        Name,
                                        Ordinal,
                                        Activated,
                                        Rank,
                                        AutoRank,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @Code,
                                        @Name,
                                        @Ordinal,
                                        @Activated,
                                        @Rank,
                                        @AutoRank,
                                        @CreatedTime,
                                        @UpdatedTime)";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, industry) > 0;
        }

        public virtual async Task<bool> UpdateAsync(Industry industry)
        {
            string query = @"    UPDATE Industries SET
			                            Name = @Name,
			                            Ordinal = @Ordinal,
			                            Activated = @Activated,
			                            Rank = @Rank,
                                        AutoRank = @AutoRank,
                                        UpdatedTime = GETDATE()
		                            WHERE
			                            Id = @Id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, industry) > 0;
        }
    }
}