using System.Text.Json.Serialization;

namespace Pl.Sas.Core.Entities.DownloadObjects
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

    #endregion

    #region StockPrice

    public class SsiStockPriceHistory
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; } = null!;
    }

    public class Data
    {
        [JsonPropertyName("stockPrice")]
        public StockPriceList StockPrice { get; set; } = null!;
    }

    public class StockPriceList
    {
        [JsonPropertyName("dataList")]
        public StockPriceSsi[] DataList { get; set; } = null!;
    }

    public class StockPriceSsi
    {
        [JsonPropertyName("tradingdate")]
        public string TradingDate { get; set; } = null!;

        [JsonPropertyName("pricechange")]
        public string PriceChange { get; set; } = null!;

        [JsonPropertyName("perpricechange")]
        public string PerPriceChange { get; set; } = null!;

        [JsonPropertyName("ceilingprice")]
        public string CeilingPrice { get; set; } = null!;

        [JsonPropertyName("floorprice")]
        public string FloorPrice { get; set; } = null!;

        [JsonPropertyName("refprice")]
        public string RefPrice { get; set; } = null!;

        [JsonPropertyName("openprice")]
        public string OpenPrice { get; set; } = null!;

        [JsonPropertyName("highestprice")]
        public string HighestPrice { get; set; } = null!;

        [JsonPropertyName("lowestprice")]
        public string LowestPrice { get; set; } = null!;

        [JsonPropertyName("closeprice")]
        public string ClosePrice { get; set; } = null!;

        [JsonPropertyName("averageprice")]
        public string AveragePrice { get; set; } = null!;

        [JsonPropertyName("closepriceadjusted")]
        public string ClosePriceAdjusted { get; set; } = null!;

        [JsonPropertyName("totalmatchvol")]
        public string TotalMatchVol { get; set; } = null!;

        [JsonPropertyName("totalmatchval")]
        public string TotalMatchVal { get; set; } = null!;

        [JsonPropertyName("totaldealval")]
        public string TotalDealVal { get; set; } = null!;

        [JsonPropertyName("totaldealvol")]
        public string TotalDealVol { get; set; } = null!;

        [JsonPropertyName("foreignbuyvoltotal")]
        public string ForeignBuyVolTotal { get; set; } = null!;

        [JsonPropertyName("foreigncurrentroom")]
        public string ForeignCurrentRoom { get; set; } = null!;

        [JsonPropertyName("foreignsellvoltotal")]
        public string ForeignSellVolTotal { get; set; } = null!;

        [JsonPropertyName("foreignbuyvaltotal")]
        public string ForeignBuyValTotal { get; set; } = null!;

        [JsonPropertyName("foreignsellvaltotal")]
        public string ForeignSellValTotal { get; set; } = null!;

        [JsonPropertyName("totalbuytrade")]
        public string TotalBuyTrade { get; set; } = null!;

        [JsonPropertyName("totalbuytradevol")]
        public string TotalBuyTradeVol { get; set; } = null!;

        [JsonPropertyName("totalselltrade")]
        public string TotalSellTrade { get; set; } = null!;

        [JsonPropertyName("totalselltradevol")]
        public string TotalSellTradeVol { get; set; } = null!;

        [JsonPropertyName("netbuysellvol")]
        public string NetBuySellVol { get; set; } = null!;

        [JsonPropertyName("netbuysellval")]
        public string NetBuySellVal { get; set; } = null!;
    }

    #endregion

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
        public string SubsectorCode { get; set; } = null!;

        [JsonPropertyName("industryname")]
        public string? IndustryName { get; set; }

        [JsonPropertyName("supersector")]
        public string? Supersector { get; set; }

        [JsonPropertyName("sector")]
        public string? Sector { get; set; }

        [JsonPropertyName("subsector")]
        public string Subsector { get; set; } = null!;

        [JsonPropertyName("foundingdate")]
        public string? FoundingDate { get; set; }

        [JsonPropertyName("chartercapital")]
        public string CharterCapital { get; set; } = null!;

        [JsonPropertyName("numberofemployee")]
        public string? NumberOfEmployee { get; set; }

        [JsonPropertyName("banknumberofbranch")]
        public string? BankNumberOfBranch { get; set; }

        [JsonPropertyName("companyprofile")]
        public string CompanyProfile { get; set; } = null!;

        [JsonPropertyName("listingdate")]
        public string? ListingDate { get; set; }

        [JsonPropertyName("exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("firstprice")]
        public string FirstPrice { get; set; } = null!;

        [JsonPropertyName("issueshare")]
        public string IssueShare { get; set; } = null!;

        [JsonPropertyName("listedvalue")]
        public string ListedValue { get; set; } = null!;

        [JsonPropertyName("companyname")]
        public string? CompanyName { get; set; }
    }

    public class Companystatistics
    {
        [JsonPropertyName("marketcap")]
        public string MarketCap { get; set; } = null!;

        [JsonPropertyName("sharesoutstanding")]
        public string SharesOutStanding { get; set; } = null!;

        [JsonPropertyName("bv")]
        public string BV { get; set; } = null!;

        [JsonPropertyName("beta")]
        public string BETA { get; set; } = null!;

        [JsonPropertyName("eps")]
        public string EPS { get; set; } = null!;

        [JsonPropertyName("dilutedeps")]
        public string DilutedEps { get; set; } = null!;

        [JsonPropertyName("pe")]
        public string PE { get; set; } = null!;

        [JsonPropertyName("pb")]
        public string PB { get; set; } = null!;

        [JsonPropertyName("dividendyield")]
        public string DividendYield { get; set; } = null!;

        [JsonPropertyName("totalrevenue")]
        public string TotalRevenue { get; set; } = null!;

        [JsonPropertyName("profit")]
        public string Profit { get; set; } = null!;

        [JsonPropertyName("asset")]
        public string Asset { get; set; } = null!;

        [JsonPropertyName("roe")]
        public string Roe { get; set; } = null!;

        [JsonPropertyName("roa")]
        public string Roa { get; set; } = null!;

        [JsonPropertyName("npl")]
        public string Npl { get; set; } = null!;

        [JsonPropertyName("financialleverage")]
        public string FinanciallEverage { get; set; } = null!;
    }

    #endregion

    #region Leadership

    public class SsiLeadership
    {
        [JsonPropertyName("data")]
        public SsiLeadershipData Data { get; set; } = null!;
    }

    public class SsiLeadershipData
    {
        [JsonPropertyName("leaderships")]
        public Leaderships Leaderships { get; set; } = null!;
    }

    public class Leaderships
    {
        [JsonPropertyName("datas")]
        public LeadershipJson[] Datas { get; set; } = null!;

    }

    public class LeadershipJson
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = null!;

        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("positionname")]
        public string PositionName { get; set; } = null!;

        [JsonPropertyName("positionlevel")]
        public string PositionLevel { get; set; } = null!;
    }

    #endregion

    #region Capital and Dividend

    public class SsiCapitalAndDividend
    {
        [JsonPropertyName("data")]
        public CapitalAndDividendData Data { get; set; } = null!;
    }

    public class CapitalAndDividendData
    {
        [JsonPropertyName("capAndDividend")]
        public Capanddividend CapAndDividend { get; set; } = null!;
    }

    public class Capanddividend
    {
        [JsonPropertyName("tabcapitaldividendresponse")]
        public Tabcapitaldividendresponse TabcapitalDividendResponse { get; set; } = null!;
    }

    public class Tabcapitaldividendresponse
    {
        [JsonPropertyName("datagroup")]
        public Datagroup DataGroup { get; set; } = null!;
    }

    public class Datagroup
    {
        [JsonPropertyName("assetlistList")]
        public Assetlistlist[] AssetlistList { get; set; } = null!;

        [JsonPropertyName("cashdividendlistList")]
        public Cashdividendlistlist[] CashdividendlistList { get; set; } = null!;

        [JsonPropertyName("ownercapitallistList")]
        public Ownercapitallistlist[] OwnercapitallistList { get; set; } = null!;
    }

    public class Assetlistlist
    {
        [JsonPropertyName("year")]
        public string Year { get; set; } = null!;

        [JsonPropertyName("asset")]
        public string Asset { get; set; } = null!;
    }

    public class Cashdividendlistlist
    {
        [JsonPropertyName("year")]
        public string Year { get; set; } = null!;

        [JsonPropertyName("valuepershare")]
        public string ValuePershare { get; set; } = null!;
    }

    public class Ownercapitallistlist
    {
        [JsonPropertyName("year")]
        public string Year { get; set; } = null!;

        [JsonPropertyName("ownercapital")]
        public string OwnerCapital { get; set; } = null!;
    }

    #endregion

    #region Financial Indicator

    public class SsiFinancialIndicator
    {
        [JsonPropertyName("data")]
        public FinancialIndicatorData Data { get; set; } = null!;
    }

    public class FinancialIndicatorData
    {
        [JsonPropertyName("financialIndicator")]
        public Financialindicator FinancialIndicator { get; set; } = null!;
    }

    public class Financialindicator
    {
        [JsonPropertyName("dataList")]
        public FinancialindicatorDatalist[] DataList { get; set; } = null!;
    }

    public class FinancialindicatorDatalist
    {
        [JsonPropertyName("revenue")]
        public string Revenue { get; set; } = null!;

        [JsonPropertyName("profit")]
        public string Profit { get; set; } = null!;

        [JsonPropertyName("yearreport")]
        public string YearReport { get; set; } = null!;

        [JsonPropertyName("lengthreport")]
        public string LengthReport { get; set; } = null!;

        [JsonPropertyName("eps")]
        public string Eps { get; set; } = null!;

        [JsonPropertyName("dilutedeps")]
        public string DilutedEps { get; set; } = null!;

        [JsonPropertyName("pe")]
        public string Pe { get; set; } = null!;

        [JsonPropertyName("dilutedpe")]
        public string DilutedPe { get; set; } = null!;

        [JsonPropertyName("roe")]
        public string Roe { get; set; } = null!;

        [JsonPropertyName("roa")]
        public string Roa { get; set; } = null!;

        [JsonPropertyName("roic")]
        public string Roic { get; set; } = null!;

        [JsonPropertyName("grossprofitmargin")]
        public string GrossProfitMargin { get; set; } = null!;

        [JsonPropertyName("netprofitmargin")]
        public string NetProfitMargin { get; set; } = null!;

        [JsonPropertyName("debtequity")]
        public string DebtEquity { get; set; } = null!;

        [JsonPropertyName("debtasset")]
        public string DebtAsset { get; set; } = null!;

        [JsonPropertyName("quickratio")]
        public string QuickRatio { get; set; } = null!;

        [JsonPropertyName("currentratio")]
        public string CurrentRatio { get; set; } = null!;

        [JsonPropertyName("pb")]
        public string Pb { get; set; } = null!;
    }

    #endregion

    #region Corporate Action

    public class SsiCorporateAction
    {
        [JsonPropertyName("data")]
        public SsiCorporateActions Data { get; set; } = null!;
    }

    public class SsiCorporateActions
    {
        [JsonPropertyName("corporateActions")]
        public CorporateActionsApi CorporateActions { get; set; } = null!;
    }

    public class CorporateActionsApi
    {
        [JsonPropertyName("dataList")]
        public CorporateActionApi[] DataList { get; set; } = null!;
    }

    public class CorporateActionApi
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = null!;

        [JsonPropertyName("eventname")]
        public string EventName { get; set; } = null!;

        [JsonPropertyName("exrightdate")]
        public string ExrightDate { get; set; } = null!;

        [JsonPropertyName("recorddate")]
        public string RecordDate { get; set; } = null!;

        [JsonPropertyName("issuedate")]
        public string IssueDate { get; set; } = null!;

        [JsonPropertyName("eventtitle")]
        public string EventTitle { get; set; } = null!;

        [JsonPropertyName("publicdate")]
        public string PublicDate { get; set; } = null!;

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = null!;

        [JsonPropertyName("eventlistcode")]
        public string EventListCode { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("ratio")]
        public string Ratio { get; set; } = null!;

        [JsonPropertyName("eventdescription")]
        public string EventDescription { get; set; } = null!;

        [JsonPropertyName("eventcode")]
        public string EventCode { get; set; } = null!;
    }

    #endregion

    #region Fiintrading Evaluate

    public class FiintradingEvaluate
    {

        [JsonPropertyName("items")]
        public FiinEvaluateItem[] Items { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("errors")]
        public object Errors { get; set; } = null!;
    }

    public class FiinEvaluateItem
    {
        [JsonPropertyName("organCode")]
        public string OrganCode { get; set; } = null!;

        [JsonPropertyName("icbRank")]
        public int? IcbRank { get; set; }

        [JsonPropertyName("icbTotalRanked")]
        public int? IcbTotalRanked { get; set; }

        [JsonPropertyName("indexRank")]
        public int? IndexRank { get; set; }

        [JsonPropertyName("indexTotalRanked")]
        public int? IndexTotalRanked { get; set; }

        [JsonPropertyName("icbCode")]
        public string IcbCode { get; set; } = null!;

        [JsonPropertyName("comGroupCode")]
        public string ComGroupCode { get; set; } = null!;

        [JsonPropertyName("growth")]
        public string Growth { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("momentum")]
        public string Momentum { get; set; } = null!;

        [JsonPropertyName("vgm")]
        public string Vgm { get; set; } = null!;

        [JsonPropertyName("controlStatusCode")]
        public int? ControlStatusCode { get; set; }

        [JsonPropertyName("controlStatusName")]
        public string ControlStatusName { get; set; } = null!;
    }

    #endregion

    #region Stock Recommendations

    public class VndRecommendations
    {
        [JsonPropertyName("data")]
        public VndRecommendationsItem[] Data { get; set; } = null!;
    }

    public class VndRecommendationsItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("firm")]
        public string Firm { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("reportDate")]
        public string ReportDate { get; set; } = null!;

        [JsonPropertyName("source")]
        public string Source { get; set; } = null!;

        [JsonPropertyName("analyst")]
        public string Analyst { get; set; } = null!;

        [JsonPropertyName("reportPrice")]
        public float ReportPrice { get; set; }

        [JsonPropertyName("targetPrice")]
        public float TargetPrice { get; set; }

        [JsonPropertyName("avgTargetPrice")]
        public float AvgTargetPrice { get; set; }
    }

    #endregion

    #region Vnd Stock Scorings
    public class VndStockScorings
    {
        [JsonPropertyName("data")]
        public VndStockScoringsItem[] Data { get; set; } = null!;
    }

    public class VndStockScoringsItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// 2020-06-30
        /// </summary>
        [JsonPropertyName("fiscalDate")]
        public string FiscalDate { get; set; } = null!;

        [JsonPropertyName("modelCode")]
        public string ModelCode { get; set; } = null!;

        [JsonPropertyName("criteriaCode")]
        public string CriteriaCode { get; set; } = null!;

        [JsonPropertyName("criteriaType")]
        public string CriteriaType { get; set; } = null!;

        [JsonPropertyName("criteriaName")]
        public string CriteriaName { get; set; } = null!;

        [JsonPropertyName("point")]
        public float Point { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = null!;
    }
    #endregion

    #region Transaction

    public class SsiTransaction
    {
        [JsonPropertyName("data")]
        public SsiTransactionJsonObject Data { get; set; } = null!;
    }

    public class SsiTransactionJsonObject
    {
        [JsonPropertyName("leTables")]
        public LetableTransaction[] LeTables { get; set; } = null!;
    }

    public class LetableTransaction
    {
        [JsonPropertyName("stockNo")]
        public string StockNo { get; set; } = null!;

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("vol")]
        public int Vol { get; set; }

        [JsonPropertyName("accumulatedVol")]
        public int AccumulatedVol { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = null!;

        [JsonPropertyName("side")]
        public string Side { get; set; } = null!;

        [JsonPropertyName("_ref")]
        public int RefPrice { get; set; }
    }

    #endregion

    #region Index price

    public class SsiChartPrice
    {
        [JsonPropertyName("t")]
        public long[] Time { get; set; } = null!;

        [JsonPropertyName("c")]
        public string[] Close { get; set; } = null!;

        [JsonPropertyName("o")]
        public string[] Open { get; set; } = null!;

        [JsonPropertyName("h")]
        public string[] Highest { get; set; } = null!;

        [JsonPropertyName("l")]
        public string[] Lowest { get; set; } = null!;

        [JsonPropertyName("v")]
        public string[] Volumes { get; set; } = null!;

        [JsonPropertyName("s")]
        public string Status { get; set; } = null!;
    }

    public class VndChartPrice
    {
        [JsonPropertyName("t")]
        public long[] Time { get; set; } = null!;

        [JsonPropertyName("c")]
        public float[] Close { get; set; } = null!;

        [JsonPropertyName("o")]
        public float[] Open { get; set; } = null!;

        [JsonPropertyName("h")]
        public float[] Highest { get; set; } = null!;

        [JsonPropertyName("l")]
        public float[] Lowest { get; set; } = null!;

        [JsonPropertyName("v")]
        public int[] Volumes { get; set; } = null!;

        [JsonPropertyName("s")]
        public string Status { get; set; } = null!;
    }

    #endregion

    #region VndBankInterestRate

    public class BankInterestRateObject
    {
        [JsonPropertyName("data")]
        public BankInterestRateItem[] Data { get; set; } = null!;
    }

    public class BankInterestRateItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("channel")]
        public string Channel { get; set; } = null!;

        [JsonPropertyName("effectiveDate")]
        public string EffectiveDate { get; set; } = null!;

        [JsonPropertyName("interestRate")]
        public float InterestRate { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = null!;

        [JsonPropertyName("term")]
        public float? Term { get; set; }

        [JsonPropertyName("minLimit")]
        public float? MinLimit { get; set; }

        [JsonPropertyName("maxLimit")]
        public float? MaxLimit { get; set; }

        [JsonPropertyName("paymentType")]
        public string PaymentType { get; set; } = null!;

        [JsonPropertyName("customerType")]
        public string CustomerType { get; set; } = null!;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = null!;
    }

    #endregion
}
