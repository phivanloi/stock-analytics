using System.Text.Json.Serialization;

namespace Pl.Sas.Core.Entities.Response
{
    #region All stock import

    public class SsiAllStock
    {
        [JsonPropertyName("data")]
        public SsiStockItem[] Data { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class SsiStockItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("stockNo")]
        public string StockNo { get; set; }

        [JsonPropertyName("clientName")]
        public string ClientName { get; set; }

        [JsonPropertyName("clientNameEn")]
        public string ClientNameEn { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("securityName")]
        public string SecurityName { get; set; }

        [JsonPropertyName("symbol_ref")]
        public string SymbolFef { get; set; }
    }

    #endregion All stock import

    #region StockPrice

    public class SsiStockPriceHistory
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("stockPrice")]
        public Stockprice StockPrice { get; set; }
    }

    public class Stockprice
    {
        [JsonPropertyName("dataList")]
        public StockPriceSsi[] DataList { get; set; }

        [JsonPropertyName("stockpriceresponse")]
        public Stockpriceresponse StockPriceResponse { get; set; }
    }

    public class Stockpriceresponse
    {
        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    public class Paging
    {
        [JsonPropertyName("pagesize")]
        public string PageSize { get; set; }

        [JsonPropertyName("currentpage")]
        public string CurrentPage { get; set; }

        [JsonPropertyName("totalpage")]
        public string TotalPage { get; set; }

        [JsonPropertyName("totalrow")]
        public string TotalRow { get; set; }
    }

    public class StockPriceSsi
    {
        [JsonPropertyName("tradingdate")]
        public string TradingDate { get; set; }

        [JsonPropertyName("pricechange")]
        public string PriceChange { get; set; }

        [JsonPropertyName("perpricechange")]
        public string PerPriceChange { get; set; }

        [JsonPropertyName("ceilingprice")]
        public string CeilingPrice { get; set; }

        [JsonPropertyName("floorprice")]
        public string FloorPrice { get; set; }

        [JsonPropertyName("refprice")]
        public string RefPrice { get; set; }

        [JsonPropertyName("openprice")]
        public string OpenPrice { get; set; }

        [JsonPropertyName("highestprice")]
        public string HighestPrice { get; set; }

        [JsonPropertyName("lowestprice")]
        public string LowestPrice { get; set; }

        [JsonPropertyName("closeprice")]
        public string ClosePrice { get; set; }

        [JsonPropertyName("averageprice")]
        public string AveragePrice { get; set; }

        [JsonPropertyName("closepriceadjusted")]
        public string ClosePriceAdjusted { get; set; }

        [JsonPropertyName("totalmatchvol")]
        public string TotalMatchVol { get; set; }

        [JsonPropertyName("totalmatchval")]
        public string TotalMatchVal { get; set; }

        [JsonPropertyName("totaldealval")]
        public string TotalDealVal { get; set; }

        [JsonPropertyName("totaldealvol")]
        public string TotalDealVol { get; set; }

        [JsonPropertyName("foreignbuyvoltotal")]
        public string ForeignBuyVolTotal { get; set; }

        [JsonPropertyName("foreigncurrentroom")]
        public string ForeignCurrentRoom { get; set; }

        [JsonPropertyName("foreignsellvoltotal")]
        public string ForeignSellVolTotal { get; set; }

        [JsonPropertyName("foreignbuyvaltotal")]
        public string ForeignBuyValTotal { get; set; }

        [JsonPropertyName("foreignsellvaltotal")]
        public string ForeignSellValTotal { get; set; }

        [JsonPropertyName("totalbuytrade")]
        public string TotalBuyTrade { get; set; }

        [JsonPropertyName("totalbuytradevol")]
        public string TotalBuyTradeVol { get; set; }

        [JsonPropertyName("totalselltrade")]
        public string TotalSellTrade { get; set; }

        [JsonPropertyName("totalselltradevol")]
        public string TotalSellTradeVol { get; set; }

        [JsonPropertyName("netbuysellvol")]
        public string NetBuySellVol { get; set; }

        [JsonPropertyName("netbuysellval")]
        public string NetBuySellVal { get; set; }
    }

    #endregion StockPrice

    #region Company Info

    public class SsiCompanyInfo
    {
        [JsonPropertyName("data")]
        public SsiCompanyInfoData Data { get; set; }
    }

    public class SsiCompanyInfoData
    {
        [JsonPropertyName("companyProfile")]
        public Companyprofile CompanyProfile { get; set; }

        [JsonPropertyName("companyStatistics")]
        public Companystatistics CompanyStatistics { get; set; }
    }

    public class Companyprofile
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("subsectorcode")]
        public string SubsectorCode { get; set; }

        [JsonPropertyName("industryname")]
        public string IndustryName { get; set; }

        [JsonPropertyName("supersector")]
        public string Supersector { get; set; }

        [JsonPropertyName("sector")]
        public string Sector { get; set; }

        [JsonPropertyName("subsector")]
        public string Subsector { get; set; }

        [JsonPropertyName("foundingdate")]
        public string FoundingDate { get; set; }

        [JsonPropertyName("chartercapital")]
        public string CharterCapital { get; set; }

        [JsonPropertyName("numberofemployee")]
        public string NumberOfEmployee { get; set; }

        [JsonPropertyName("banknumberofbranch")]
        public string BankNumberOfBranch { get; set; }

        [JsonPropertyName("companyprofile")]
        public string CompanyProfile { get; set; }

        [JsonPropertyName("listingdate")]
        public string ListingDate { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("firstprice")]
        public string FirstPrice { get; set; }

        [JsonPropertyName("issueshare")]
        public string IssueShare { get; set; }

        [JsonPropertyName("listedvalue")]
        public string ListedValue { get; set; }

        [JsonPropertyName("companyname")]
        public string CompanyName { get; set; }

        [JsonPropertyName("__typename")]
        public string TypeName { get; set; }
    }

    public class Companystatistics
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("ttmtype")]
        public string TtmType { get; set; }

        [JsonPropertyName("marketcap")]
        public string MarketCap { get; set; }

        [JsonPropertyName("sharesoutstanding")]
        public string SharesOutStanding { get; set; }

        [JsonPropertyName("bv")]
        public string BV { get; set; }

        [JsonPropertyName("beta")]
        public string BETA { get; set; }

        [JsonPropertyName("eps")]
        public string EPS { get; set; }

        [JsonPropertyName("dilutedeps")]
        public string DilutedEps { get; set; }

        [JsonPropertyName("pe")]
        public string PE { get; set; }

        [JsonPropertyName("pb")]
        public string PB { get; set; }

        [JsonPropertyName("dividendyield")]
        public string DividendYield { get; set; }

        [JsonPropertyName("totalrevenue")]
        public string TotalRevenue { get; set; }

        [JsonPropertyName("profit")]
        public string Profit { get; set; }

        [JsonPropertyName("asset")]
        public string Asset { get; set; }

        [JsonPropertyName("roe")]
        public string Roe { get; set; }

        [JsonPropertyName("roa")]
        public string Roa { get; set; }

        [JsonPropertyName("npl")]
        public string Npl { get; set; }

        [JsonPropertyName("financialleverage")]
        public string FinanciallEverage { get; set; }

        [JsonPropertyName("__typename")]
        public string TypeName { get; set; }
    }

    #endregion Company Info

    #region Leadership

    public class SsiLeadership
    {
        [JsonPropertyName("data")]
        public SsiLeadershipData Data { get; set; }
    }

    public class SsiLeadershipData
    {
        [JsonPropertyName("leaderships")]
        public Leaderships Leaderships { get; set; }
    }

    public class Leaderships
    {
        [JsonPropertyName("datas")]
        public LeadershipJson[] Datas { get; set; }

        [JsonPropertyName("__typename")]
        public string TypeName { get; set; }
    }

    public class LeadershipJson
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("fullname")]
        public string FullName { get; set; }

        [JsonPropertyName("positionname")]
        public string PositionName { get; set; }

        [JsonPropertyName("positionlevel")]
        public string PositionLevel { get; set; }

        [JsonPropertyName("__typename")]
        public string TypeName { get; set; }
    }

    #endregion Leadership

    #region Capital and Dividend

    public class SsiCapitalAndDividend
    {
        [JsonPropertyName("data")]
        public CapitalAndDividendData Data { get; set; }
    }

    public class CapitalAndDividendData
    {
        [JsonPropertyName("capAndDividend")]
        public Capanddividend CapAndDividend { get; set; }
    }

    public class Capanddividend
    {
        [JsonPropertyName("dataList")]
        public object[] DataList { get; set; }

        [JsonPropertyName("tabcapitaldividendresponse")]
        public Tabcapitaldividendresponse TabcapitalDividendResponse { get; set; }
    }

    public class Tabcapitaldividendresponse
    {
        [JsonPropertyName("datagroup")]
        public Datagroup DataGroup { get; set; }
    }

    public class Datagroup
    {
        [JsonPropertyName("assetlistList")]
        public Assetlistlist[] AssetlistList { get; set; }

        [JsonPropertyName("cashdividendlistList")]
        public Cashdividendlistlist[] CashdividendlistList { get; set; }

        [JsonPropertyName("ownercapitallistList")]
        public Ownercapitallistlist[] OwnercapitallistList { get; set; }
    }

    public class Assetlistlist
    {
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("asset")]
        public string Asset { get; set; }
    }

    public class Cashdividendlistlist
    {
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("valuepershare")]
        public string ValuePershare { get; set; }
    }

    public class Ownercapitallistlist
    {
        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("ownercapital")]
        public string OwnerCapital { get; set; }
    }

    #endregion Capital and Dividend

    #region Financial Indicator

    public class SsiFinancialIndicator
    {
        [JsonPropertyName("data")]
        public FinancialIndicatorData Data { get; set; }
    }

    public class FinancialIndicatorData
    {
        [JsonPropertyName("financialIndicator")]
        public Financialindicator FinancialIndicator { get; set; }
    }

    public class Financialindicator
    {
        [JsonPropertyName("dataList")]
        public FinancialindicatorDatalist[] DataList { get; set; }

        [JsonPropertyName("financialindicatorresponse")]
        public Financialindicatorresponse FinancialIndicatorResponse { get; set; }
    }

    public class Financialindicatorresponse
    {
        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    public class FinancialindicatorDatalist
    {
        [JsonPropertyName("revenue")]
        public string Revenue { get; set; }

        [JsonPropertyName("profit")]
        public string Profit { get; set; }

        [JsonPropertyName("yearreport")]
        public string YearReport { get; set; }

        [JsonPropertyName("lengthreport")]
        public string LengthReport { get; set; }

        [JsonPropertyName("eps")]
        public string Eps { get; set; }

        [JsonPropertyName("dilutedeps")]
        public string DilutedEps { get; set; }

        [JsonPropertyName("pe")]
        public string Pe { get; set; }

        [JsonPropertyName("dilutedpe")]
        public string DilutedPe { get; set; }

        [JsonPropertyName("roe")]
        public string Roe { get; set; }

        [JsonPropertyName("roa")]
        public string Roa { get; set; }

        [JsonPropertyName("roic")]
        public string Roic { get; set; }

        [JsonPropertyName("grossprofitmargin")]
        public string GrossProfitMargin { get; set; }

        [JsonPropertyName("netprofitmargin")]
        public string NetProfitMargin { get; set; }

        [JsonPropertyName("debtequity")]
        public string DebtEquity { get; set; }

        [JsonPropertyName("debtasset")]
        public string DebtAsset { get; set; }

        [JsonPropertyName("quickratio")]
        public string QuickRatio { get; set; }

        [JsonPropertyName("currentratio")]
        public string CurrentRatio { get; set; }

        [JsonPropertyName("pb")]
        public string Pb { get; set; }
    }

    #endregion Financial Indicator

    #region CorporateAction

    public class SsiCorporateAction
    {
        [JsonPropertyName("data")]
        public SsiCorporateActions Data { get; set; }
    }

    public class SsiCorporateActions
    {
        [JsonPropertyName("corporateActions")]
        public CorporateActionsApi CorporateActions { get; set; }
    }

    public class CorporateActionsApi
    {
        [JsonPropertyName("dataList")]
        public CorporateActionApi[] DataList { get; set; }

        [JsonPropertyName("corporateactionsresponse")]
        public CorporateActionsResponse CorporateActionsResponse { get; set; }
    }

    public class CorporateActionsResponse
    {
        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    public class CorporateActionApi
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("eventname")]
        public string EventName { get; set; }

        [JsonPropertyName("exrightdate")]
        public string ExrightDate { get; set; }

        [JsonPropertyName("recorddate")]
        public string RecordDate { get; set; }

        [JsonPropertyName("issuedate")]
        public string IssueDate { get; set; }

        [JsonPropertyName("eventtitle")]
        public string EventTitle { get; set; }

        [JsonPropertyName("publicdate")]
        public string PublicDate { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("eventlistcode")]
        public string EventListCode { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("ratio")]
        public string Ratio { get; set; }

        [JsonPropertyName("eventdescription")]
        public string EventDescription { get; set; }

        [JsonPropertyName("eventcode")]
        public string EventCode { get; set; }
    }

    #endregion CorporateAction

    #region Transaction

    public class SsiTransaction
    {
        [JsonPropertyName("data")]
        public SsiTransactionJsonObject Data { get; set; }
    }

    public class SsiTransactionJsonObject
    {
        [JsonPropertyName("leTables")]
        public LetableTransaction[] LeTables { get; set; }

        [JsonPropertyName("stockRealtime")]
        public StockrealtimeTransaction StockRealtime { get; set; }
    }

    public class StockrealtimeTransaction
    {
        [JsonPropertyName("stockNo")]
        public string StockNo { get; set; }

        [JsonPropertyName("ceiling")]
        public int? Ceiling { get; set; }

        [JsonPropertyName("floor")]
        public int? Floor { get; set; }

        [JsonPropertyName("refPrice")]
        public int? RefPrice { get; set; }

        [JsonPropertyName("stockSymbol")]
        public string StockSymbol { get; set; }
    }

    public class LetableTransaction
    {
        [JsonPropertyName("stockNo")]
        public string StockNo { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("vol")]
        public int Vol { get; set; }

        [JsonPropertyName("accumulatedVol")]
        public int AccumulatedVol { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("_ref")]
        public int RefPrice { get; set; }
    }

    #endregion Transaction

    #region Fiintrading Evaluate

    public class FiintradingEvaluateObject
    {
        [JsonPropertyName("page")]
        public int? Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int? PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        public int? TotalCount { get; set; }

        [JsonPropertyName("items")]
        public FiinEvaluateItem[] Items { get; set; }

        [JsonPropertyName("packageId")]
        public object PackageId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("errors")]
        public object Errors { get; set; }
    }

    public class FiinEvaluateItem
    {
        [JsonPropertyName("organCode")]
        public string OrganCode { get; set; }

        [JsonPropertyName("icbRank")]
        public int? IcbRank { get; set; }

        [JsonPropertyName("icbTotalRanked")]
        public int? IcbTotalRanked { get; set; }

        [JsonPropertyName("indexRank")]
        public int? IndexRank { get; set; }

        [JsonPropertyName("indexTotalRanked")]
        public int? IndexTotalRanked { get; set; }

        [JsonPropertyName("icbCode")]
        public string IcbCode { get; set; }

        [JsonPropertyName("comGroupCode")]
        public string ComGroupCode { get; set; }

        [JsonPropertyName("growth")]
        public string Growth { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("momentum")]
        public string Momentum { get; set; }

        [JsonPropertyName("vgm")]
        public string Vgm { get; set; }

        [JsonPropertyName("controlStatusCode")]
        public int? ControlStatusCode { get; set; }

        [JsonPropertyName("controlStatusName")]
        public string ControlStatusName { get; set; }
    }

    #endregion

    #region VndIndex

    public class VndIndexBlockObject
    {
        [JsonPropertyName("t")]
        public int[] Time { get; set; }

        [JsonPropertyName("c")]
        public float[] Close { get; set; }

        [JsonPropertyName("o")]
        public float[] Open { get; set; }

        [JsonPropertyName("h")]
        public float[] Highest { get; set; }

        [JsonPropertyName("l")]
        public float[] Lowest { get; set; }

        [JsonPropertyName("v")]
        public int[] Volumes { get; set; }

        [JsonPropertyName("s")]
        public string Status { get; set; }
    }

    #endregion

    #region VndBankInterestRate

    public class BankInterestRateObject
    {
        [JsonPropertyName("data")]
        public BankInterestRateItem[] Data { get; set; }

        [JsonPropertyName("currentPage")]
        public int? CurrentPage { get; set; }

        [JsonPropertyName("size")]
        public int? Size { get; set; }

        [JsonPropertyName("totalElements")]
        public int? TotalElements { get; set; }

        [JsonPropertyName("totalPages")]
        public int? TotalPages { get; set; }
    }

    public class BankInterestRateItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("effectiveDate")]
        public string EffectiveDate { get; set; }

        [JsonPropertyName("interestRate")]
        public float InterestRate { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("term")]
        public float? Term { get; set; }

        [JsonPropertyName("minLimit")]
        public float? MinLimit { get; set; }

        [JsonPropertyName("maxLimit")]
        public float? MaxLimit { get; set; }

        [JsonPropertyName("paymentType")]
        public string PaymentType { get; set; }

        [JsonPropertyName("customerType")]
        public string CustomerType { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }

    #endregion

    #region VndEconomicIndicatiorOfTopIndexs


    public class VndEconomicIndicatiorOfTopIndexs
    {
        [JsonPropertyName("data")]
        public EconomicIndicatiorItem[] Data { get; set; }
    }

    public class EconomicIndicatiorItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("group")]
        public string Group { get; set; }

        [JsonPropertyName("reportDate")]
        public string ReportDate { get; set; }

        [JsonPropertyName("itemCode")]
        public string ItemCode { get; set; }

        [JsonPropertyName("itemName")]
        public string ItemName { get; set; }

        [JsonPropertyName("value")]
        public float Value { get; set; }
    }


    #endregion

    #region EconomicIndex

    public class SourceEconomicIndex
    {
        public int Index { get; set; }
        public string IndustriesUp { get; set; }
        public string IndustriesDown { get; set; }
        public string SymbolsUp { get; set; }
        public string SymbolsDown { get; set; }
    }

    #endregion

    #region Commodity

    public class VndCommodity
    {
        [JsonPropertyName("data")]
        public VndCommodityItem[] Data { get; set; }

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }

    public class VndCommodityItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("period")]
        public string Period { get; set; }

        [JsonPropertyName("price")]
        public float Price { get; set; }

        [JsonPropertyName("change")]
        public float Change { get; set; }

        [JsonPropertyName("changePct")]
        public float ChangePct { get; set; }

        [JsonPropertyName("lastUpdated")]
        public string LastUpdated { get; set; }
    }
    #endregion

    #region VndSignal

    public class VndSignalResponse
    {
        [JsonPropertyName("data")]
        public VndSignalItem[] Data { get; set; }

        /// <summary>
        /// stockSignal
        /// </summary>
        [JsonPropertyName("_object")]
        public string Object { get; set; }
    }

    public class VndSignalItem
    {
        /// <summary>
        /// 2021-08-20
        /// </summary>
        [JsonPropertyName("data")]
        public string Date { get; set; }

        [JsonPropertyName("indicators")]
        public VndSignalIndicator[] Indicators { get; set; }

        /// <summary>
        /// cipShort
        /// </summary>
        [JsonPropertyName("strategy")]
        public string Strategy { get; set; }

        /// <summary>
        /// HPG
        /// </summary>
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        /// <summary>
        /// 14:45:01
        /// </summary>
        [JsonPropertyName("time")]
        public string Time { get; set; }

        /// <summary>
        /// NEUTRAL
        /// </summary>
        [JsonPropertyName("totalSignal")]
        public string TotalSignal { get; set; }
    }

    public class VndSignalIndicator
    {
        /// <summary>
        /// SMA
        /// </summary>
        [JsonPropertyName("indicator")]
        public string Indicator { get; set; }

        /// <summary>
        /// Tín hiệu SMA(5)
        /// </summary>
        [JsonPropertyName("indicatorName")]
        public string IndicatorName { get; set; }

        [JsonPropertyName("lastest")]
        public VndSignalLastest[] Lastest { get; set; }

        /// <summary>
        /// 5
        /// </summary>
        [JsonPropertyName("period")]
        public string Period { get; set; }

        /// <summary>
        /// SELL,NEUTRAL,BUY
        /// </summary>
        [JsonPropertyName("signal")]
        public string Signal { get; set; }
    }

    public class VndSignalLastest
    {
        /// <summary>
        /// SMA
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 49.525
        /// </summary>
        [JsonPropertyName("value")]
        public float? Value { get; set; }
    }

    #endregion

    #region Stock Recommendations

    public class VndRecommendations
    {
        [JsonPropertyName("data")]
        public VndRecommendationsItem[] Data { get; set; }

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }

    public class VndRecommendationsItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("firm")]
        public string Firm { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("reportDate")]
        public string ReportDate { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("analyst")]
        public string Analyst { get; set; }

        [JsonPropertyName("reportPrice")]
        public float ReportPrice { get; set; }

        [JsonPropertyName("targetPrice")]
        public float TargetPrice { get; set; }

        [JsonPropertyName("avgTargetPrice")]
        public float AvgTargetPrice { get; set; }
    }

    #endregion

    #region VndStockScorings
    public class VndStockScorings
    {
        [JsonPropertyName("data")]
        public VndStockScoringsItem[] Data { get; set; }
    }

    public class VndStockScoringsItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// 2020-06-30
        /// </summary>
        [JsonPropertyName("fiscalDate")]
        public string FiscalDate { get; set; }

        [JsonPropertyName("modelCode")]
        public string ModelCode { get; set; }

        [JsonPropertyName("criteriaCode")]
        public string CriteriaCode { get; set; }

        [JsonPropertyName("criteriaType")]
        public string CriteriaType { get; set; }

        [JsonPropertyName("criteriaName")]
        public string CriteriaName { get; set; }

        [JsonPropertyName("point")]
        public float Point { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }
    }
    #endregion
}