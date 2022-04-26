using System.Text.Json.Serialization;

namespace Pl.Sas.Core.Entities
{
    #region All stock import

    public class SsiAllStock
    {
        [JsonPropertyName("data")]
        public SsiStockItem[] Data { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    public class SsiStockItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; } = null!;

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("stockNo")]
        public string StockNo { get; set; } = null!;

        [JsonPropertyName("clientName")]
        public string ClientName { get; set; } = null!;

        [JsonPropertyName("clientNameEn")]
        public string ClientNameEn { get; set; } = null!;

        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("securityName")]
        public string SecurityName { get; set; } = null!;
    }

    #endregion All stock import

    #region Company Info

    public class SsiCompanyInfo
    {
        [JsonPropertyName("data")]
        public SsiCompanyInfoData Data { get; set; } = null!;
    }

    public class SsiCompanyInfoData
    {
        [JsonPropertyName("companyProfile")]
        public Companyprofile CompanyProfile { get; set; } = null!;

        [JsonPropertyName("companyStatistics")]
        public Companystatistics CompanyStatistics { get; set; } = null!;
    }

    public class Companyprofile
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = null!;

        [JsonPropertyName("subsectorcode")]
        public string? SubsectorCode { get; set; }

        [JsonPropertyName("industryname")]
        public string? IndustryName { get; set; }

        [JsonPropertyName("supersector")]
        public string? Supersector { get; set; }

        [JsonPropertyName("sector")]
        public string? Sector { get; set; }

        [JsonPropertyName("subsector")]
        public string? Subsector { get; set; }

        [JsonPropertyName("foundingdate")]
        public string? FoundingDate { get; set; }

        [JsonPropertyName("chartercapital")]
        public string? CharterCapital { get; set; }

        [JsonPropertyName("numberofemployee")]
        public string? NumberOfEmployee { get; set; }

        [JsonPropertyName("banknumberofbranch")]
        public string? BankNumberOfBranch { get; set; }

        [JsonPropertyName("companyprofile")]
        public string? CompanyProfile { get; set; }

        [JsonPropertyName("listingdate")]
        public string? ListingDate { get; set; }

        [JsonPropertyName("exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("firstprice")]
        public string? FirstPrice { get; set; }

        [JsonPropertyName("issueshare")]
        public string? IssueShare { get; set; }

        [JsonPropertyName("listedvalue")]
        public string? ListedValue { get; set; }

        [JsonPropertyName("companyname")]
        public string? CompanyName { get; set; }
    }

    public class Companystatistics
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = null!;

        [JsonPropertyName("marketcap")]
        public string? MarketCap { get; set; }

        [JsonPropertyName("sharesoutstanding")]
        public string? SharesOutStanding { get; set; }

        [JsonPropertyName("bv")]
        public string? BV { get; set; }

        [JsonPropertyName("beta")]
        public string? BETA { get; set; }

        [JsonPropertyName("eps")]
        public string? EPS { get; set; }

        [JsonPropertyName("dilutedeps")]
        public string? DilutedEps { get; set; }

        [JsonPropertyName("pe")]
        public string? PE { get; set; }

        [JsonPropertyName("pb")]
        public string? PB { get; set; }

        [JsonPropertyName("dividendyield")]
        public string? DividendYield { get; set; }

        [JsonPropertyName("totalrevenue")]
        public string? TotalRevenue { get; set; }

        [JsonPropertyName("profit")]
        public string? Profit { get; set; }

        [JsonPropertyName("asset")]
        public string? Asset { get; set; }

        [JsonPropertyName("roe")]
        public string? Roe { get; set; }

        [JsonPropertyName("roa")]
        public string? Roa { get; set; }

        [JsonPropertyName("npl")]
        public string? Npl { get; set; }

        [JsonPropertyName("financialleverage")]
        public string? FinanciallEverage { get; set; }
    }

    #endregion Company Info
}
