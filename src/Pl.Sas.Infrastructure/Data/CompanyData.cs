using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.ComponentModel;
using System.Data;

namespace Pl.Sas.Infrastructure.Data
{
    public class CompanyData : BaseData, ICompanyData
    {
        private readonly ILogger<CompanyData> _logger;
        private readonly IAsyncCacheService _asyncCacheService;

        public CompanyData(
            IAsyncCacheService asyncCacheService,
            ILogger<CompanyData> logger,
            IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
            _asyncCacheService = asyncCacheService;
            _logger = logger;
        }

        public virtual async Task BulkInserAsync(List<Company> companies)
        {
            if (companies is null || companies.Count <= 0)
            {
                return;
            }

            var dataTableTemp = companies.ToDataTable();

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
                    sqlBulkCopy.DestinationTableName = "Companies";
                    await sqlBulkCopy.WriteToServerAsync(dataTableTemp);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company data error => BulkInserCompanyAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task BulkUpdateAsync(List<Company> companies)
        {
            if (companies is null || companies.Count <= 0)
            {
                return;
            }

            var dataTableUpdate = new DataTable();
            dataTableUpdate.Columns.Add("Id");
            dataTableUpdate.Columns.Add("Symbol");
            dataTableUpdate.Columns.Add("SubsectorCode");
            dataTableUpdate.Columns.Add("IndustryName");
            dataTableUpdate.Columns.Add("Supersector");
            dataTableUpdate.Columns.Add("Sector");
            dataTableUpdate.Columns.Add("Subsector");
            dataTableUpdate.Columns.Add(new DataColumn("FoundingDate", typeof(DateTime)) { AllowDBNull = true });
            dataTableUpdate.Columns.Add(new DataColumn("ListingDate", typeof(DateTime)) { AllowDBNull = true });
            dataTableUpdate.Columns.Add("CharterCapital", typeof(float));
            dataTableUpdate.Columns.Add("NumberOfEmployee", typeof(int));
            dataTableUpdate.Columns.Add("BankNumberOfBranch", typeof(int));
            dataTableUpdate.Columns.Add("CompanyProfile", typeof(byte[]));
            dataTableUpdate.Columns.Add("Exchange");
            dataTableUpdate.Columns.Add("FirstPrice", typeof(float));
            dataTableUpdate.Columns.Add("IssueShare", typeof(float));
            dataTableUpdate.Columns.Add("ListedValue", typeof(float));
            dataTableUpdate.Columns.Add("CompanyName");
            dataTableUpdate.Columns.Add("MarketCap", typeof(float));
            dataTableUpdate.Columns.Add("SharesOutStanding", typeof(float));
            dataTableUpdate.Columns.Add("Bv", typeof(float));
            dataTableUpdate.Columns.Add("Beta", typeof(float));
            dataTableUpdate.Columns.Add("Eps", typeof(float));
            dataTableUpdate.Columns.Add("DilutedEps", typeof(float));
            dataTableUpdate.Columns.Add("Pe", typeof(float));
            dataTableUpdate.Columns.Add("Pb", typeof(float));
            dataTableUpdate.Columns.Add("DividendYield", typeof(float));
            dataTableUpdate.Columns.Add("TotalRevenue", typeof(float));
            dataTableUpdate.Columns.Add("Profit", typeof(float));
            dataTableUpdate.Columns.Add("Asset", typeof(float));
            dataTableUpdate.Columns.Add("Roe", typeof(float));
            dataTableUpdate.Columns.Add("Roa", typeof(float));
            dataTableUpdate.Columns.Add("Npl", typeof(float));
            dataTableUpdate.Columns.Add("FinanciallEverage", typeof(float));

            var properties = TypeDescriptor.GetProperties(typeof(Company));
            foreach (var company in companies)
            {
                DataRow row = dataTableUpdate.NewRow();
                foreach (DataColumn column in dataTableUpdate.Columns)
                {
                    row[column.ColumnName] = properties[column.ColumnName]?.GetValue(company) ?? DBNull.Value;
                }
                dataTableUpdate.Rows.Add(row);
            }
            var tableName = Utilities.RandomString(15);
            var createTempTableCommand = $@" CREATE TABLE {tableName}
	                                            (Id nvarchar(22) NOT NULL,
                                                Symbol nvarchar(16) NOT NULL,
	                                            SubsectorCode nvarchar(16) NULL,
                                                IndustryName nvarchar(128) NULL,
                                                Supersector nvarchar(128) NULL,
                                                Sector nvarchar(128) NULL,
                                                Subsector nvarchar(16) NULL,
                                                FoundingDate datetime NULL,
                                                ListingDate datetime NULL,
                                                CharterCapital decimal(35, 10) NOT NULL,
                                                NumberOfEmployee int NOT NULL,
                                                BankNumberOfBranch int NOT NULL,
                                                CompanyProfile varbinary(max) NULL,
                                                Exchange nvarchar(16) NULL,
                                                FirstPrice decimal(35, 10) NOT NULL,
                                                IssueShare bigint NOT NULL,
                                                ListedValue decimal(35, 10) NOT NULL,
                                                CompanyName nvarchar(256) NULL,
                                                MarketCap decimal(35, 10) NOT NULL,
                                                SharesOutStanding decimal(35, 10) NOT NULL,
                                                Bv decimal(35, 10) NOT NULL,
                                                Beta decimal(35, 10) NOT NULL,
                                                Eps decimal(35, 10) NOT NULL,
                                                DilutedEps decimal(35, 10) NOT NULL,
                                                Pe decimal(35, 10) NOT NULL,
                                                Pb decimal(35, 10) NOT NULL,
                                                DividendYield decimal(35, 10) NOT NULL,
                                                TotalRevenue decimal(35, 10) NOT NULL,
                                                Profit decimal(35, 10) NOT NULL,
                                                Asset decimal(35, 10) NOT NULL,
                                                Roe decimal(35, 10) NOT NULL,
                                                Roa decimal(35, 10) NOT NULL,
                                                Npl decimal(35, 10) NOT NULL,
                                                FinanciallEverage decimal(35, 10) NOT NULL)";

            var updateAndDropTableCommand = $@" UPDATE
                                                    Companies
                                                SET
                                                    Companies.Symbol = Tcu.Symbol,
                                                    Companies.SubsectorCode = Tcu.SubsectorCode,
                                                    Companies.IndustryName = Tcu.IndustryName,
                                                    Companies.Supersector = Tcu.Supersector,
                                                    Companies.Sector = Tcu.Sector,
                                                    Companies.Subsector = Tcu.Subsector,
                                                    Companies.FoundingDate = Tcu.FoundingDate,
                                                    Companies.CharterCapital = Tcu.CharterCapital,
                                                    Companies.NumberOfEmployee = Tcu.NumberOfEmployee,
                                                    Companies.BankNumberOfBranch = Tcu.BankNumberOfBranch,
                                                    Companies.CompanyProfile = Tcu.CompanyProfileZip,
                                                    Companies.ListingDate = Tcu.ListingDate,
                                                    Companies.Exchange = Tcu.Exchange,
                                                    Companies.FirstPrice = Tcu.FirstPrice,
                                                    Companies.IssueShare = Tcu.IssueShare,
                                                    Companies.ListedValue = Tcu.ListedValue,
                                                    Companies.CompanyName = Tcu.CompanyName,
                                                    Companies.MarketCap = Tcu.MarketCap,
                                                    Companies.SharesOutStanding = Tcu.SharesOutStanding,
                                                    Companies.Bv = Tcu.Bv,
                                                    Companies.Beta = Tcu.Beta,
                                                    Companies.Eps = Tcu.Eps,
                                                    Companies.DilutedEps = Tcu.DilutedEps,
                                                    Companies.Pe = Tcu.Pe,
                                                    Companies.Pb = Tcu.Pb,
                                                    Companies.DividendYield = Tcu.DividendYield,
                                                    Companies.TotalRevenue = Tcu.TotalRevenue,
                                                    Companies.Profit = Tcu.Profit,
                                                    Companies.Asset = Tcu.Asset,
                                                    Companies.Roe = Tcu.Roe,
                                                    Companies.Roa = Tcu.Roa,
                                                    Companies.Npl = Tcu.Npl,
                                                    Companies.FinanciallEverage = Tcu.FinanciallEverage,
                                                    Companies.UpdatedTime = GETDATE()
                                                FROM
                                                    Companies Com
                                                INNER JOIN
                                                    {tableName} Tcu
                                                ON
                                                    Com.Id = Tcu.Id;
                                                DROP TABLE {tableName};";

            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            connection.Open();
            using var tran = connection.BeginTransaction();
            try
            {
                using (SqlCommand command = new(createTempTableCommand, connection, tran))
                {
                    await command.ExecuteNonQueryAsync();

                    using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran))
                    {
                        bulk.DestinationTableName = tableName;
                        await bulk.WriteToServerAsync(dataTableUpdate);
                    }

                    command.CommandText = updateAndDropTableCommand;
                    await command.ExecuteNonQueryAsync();
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Company data error => BulkUpdateCompanyAsync");
                tran.Rollback();
                throw;
            }
        }

        public virtual async Task<bool> InsertAsync(Company company)
        {
            var query = $@"   INSERT INTO Companies
                                        (Id,
                                        Symbol,
                                        SubsectorCode,
                                        IndustryName,
                                        Supersector,
                                        Sector,
                                        Subsector,
                                        FoundingDate,
                                        CharterCapital,
                                        NumberOfEmployee,
                                        BankNumberOfBranch,
                                        CompanyProfile,
                                        ListingDate,
                                        Exchange,
                                        FirstPrice,
                                        IssueShare,
                                        ListedValue,
                                        CompanyName,
                                        MarketCap,
                                        SharesOutStanding,
                                        Bv,
                                        Beta,
                                        Eps,
                                        DilutedEps,
                                        Pe,
                                        Pb,
                                        DividendYield,
                                        TotalRevenue,
                                        Profit,
                                        Asset,
                                        Roe,
                                        Roa,
                                        Npl,
                                        FinanciallEverage,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @Symbol,
                                        @SubsectorCode,
                                        @IndustryName,
                                        @Supersector,
                                        @Sector,
                                        @Subsector,
                                        @FoundingDate,
                                        @CharterCapital,
                                        @NumberOfEmployee,
                                        @BankNumberOfBranch,
                                        @CompanyProfile,
                                        @ListingDate,
                                        @Exchange,
                                        @FirstPrice,
                                        @IssueShare,
                                        @ListedValue,
                                        @CompanyName,
                                        @MarketCap,
                                        @SharesOutStanding,
                                        @Bv,
                                        @Beta,
                                        @Eps,
                                        @DilutedEps,
                                        @Pe,
                                        @Pb,
                                        @DividendYield,
                                        @TotalRevenue,
                                        @Profit,
                                        @Asset,
                                        @Roe,
                                        @Roa,
                                        @Npl,
                                        @FinanciallEverage,
                                        @CreatedTime,
                                        @UpdatedTime)";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, company) > 0;
        }

        public virtual async Task<bool> UpdateAsync(Company company)
        {
            var query = $@"   UPDATE Companies SET
                                Symbol = @Symbol,
                                SubsectorCode = @SubsectorCode,
                                IndustryName = @IndustryName,
                                Supersector = @Supersector,
                                Sector = @Sector,
                                Subsector = @Subsector,
                                FoundingDate = @FoundingDate,
                                CharterCapital = @CharterCapital,
                                NumberOfEmployee = @NumberOfEmployee,
                                BankNumberOfBranch = @BankNumberOfBranch,
                                CompanyProfile = @CompanyProfile,
                                ListingDate = @ListingDate,
                                Exchange = @Exchange,
                                FirstPrice = @FirstPrice,
                                IssueShare = @IssueShare,
                                ListedValue = @ListedValue,
                                CompanyName = @CompanyName,
                                MarketCap = @MarketCap,
                                SharesOutStanding = @SharesOutStanding,
                                Bv = @bv,
                                Beta = @Beta,
                                Eps = @Eps,
                                DilutedEps = @DilutedEps,
                                Pe = @Pe,
                                Pb = @Pb,
                                DividendYield = @DividendYield,
                                TotalRevenue = @TotalRevenue,
                                Profit = @Profit,
                                Asset = @Asset,
                                Roe = @Roe,
                                Roa = @Roa,
                                Npl = @Npl,
                                FinanciallEverage = @FinanciallEverage,
                                UpdatedTime = GETDATE()
                            WHERE
                                Id = @Id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, company) > 0;
        }

        public virtual async Task<List<Company>> FindAllAsync(string? subsectorCode = null)
        {
            var query = "SELECT * FROM Companies ";
            if (!string.IsNullOrEmpty(subsectorCode))
            {
                query += " WHERE SubsectorCode = @subsectorCode ";
            }
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return (await connection.QueryAsync<Company>(query, new { subsectorCode })).AsList();
        }

        public virtual async Task<Company> FindABySymbolAsync(string symbol)
        {
            Guard.Against.NullOrEmpty(symbol, nameof(symbol));
            var query = "SELECT * FROM Companies WHERE Symbol = @symbol";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.QueryFirstOrDefaultAsync<Company>(query, new { symbol });
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var query = "DELETE Companies WHERE Id = @id";
            using SqlConnection connection = new(_connectionStrings.MarketConnection);
            return await connection.ExecuteAsync(query, new { id }) > 0;
        }

        public virtual async Task<List<Company>?> CacheFindAllForAnalyticsAsync(string? subsectorCode = null)
        {
            var cacheKey = $"{Constants.CompanyCachePrefix}-CFAFAA-SC{subsectorCode}";
            return await _asyncCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var query = @"  SELECT
                                    Symbol,
                                    SubsectorCode,
                                    CharterCapital,
                                    NumberOfEmployee,
                                    BankNumberOfBranch,
                                    MarketCap,
                                    SharesOutStanding,
                                    Bv,
                                    Beta,
                                    Eps,
                                    DilutedEps,
                                    Pe,
                                    Pb,
                                    DividendYield,
                                    TotalRevenue,
                                    Profit,
                                    Asset,
                                    Roe,
                                    Asset,
                                    Roa,
                                    Npl,
                                    FinanciallEverage
                                FROM
                                    Companies ";
                if (!string.IsNullOrEmpty(subsectorCode))
                {
                    query += " WHERE SubsectorCode = @subsectorCode ";
                }
                using SqlConnection connection = new(_connectionStrings.MarketConnection);
                return (await connection.QueryAsync<Company>(query, new { subsectorCode })).AsList();
            }, Constants.DefaultCacheTime * 60 * 24);

        }
    }
}