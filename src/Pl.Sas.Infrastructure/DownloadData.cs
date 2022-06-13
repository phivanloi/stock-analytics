using Pl.Sas.Core.Entities.DownloadObjects;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Helper;
using System.Text;
using System.Text.Json;

namespace Pl.Sas.Infrastructure
{
    public class DownloadData : IDownloadData
    {
        private readonly HttpClient _ssiHttpClient;
        private readonly HttpClient _vndHttpClient;
        private readonly HttpClient _fiinHttpClient;
        private readonly HttpClient _vpsHttpClient;
        public DownloadData(IHttpClientFactory httpClientFactory)
        {
            _ssiHttpClient = httpClientFactory.CreateClient("ssidownloader");
            _vndHttpClient = httpClientFactory.CreateClient("vnddownloader");
            _fiinHttpClient = httpClientFactory.CreateClient("fiindownloader");
            _vpsHttpClient = httpClientFactory.CreateClient("vpsdownloader");
        }

        public virtual async Task<SsiAllStock?> DownloadInitialMarketStockAsync()
        {
            var requestUrl = "https://iboard.ssi.com.vn/dchart/api/1.1/defaultAllStocks";
            return await _ssiHttpClient.GetJsonAsync<SsiAllStock>(requestUrl);
        }

        public virtual async Task<SsiCompanyInfo?> DownloadCompanyInfoAsync(string symbol)
        {
            var requestUrl = "https://finfo-iboard.ssi.com.vn/graphql";
            var mainInfoQuery = new
            {
                operationName = "companyProfile",
                variables = new
                {
                    symbol,
                    language = "vn"
                },
                query = "query companyProfile($symbol: String!, $language: String) { companyProfile(symbol: $symbol, language: $language) { symbol subsectorcode industryname supersector sector subsector foundingdate chartercapital numberofemployee banknumberofbranch companyprofile listingdate exchange firstprice issueshare listedvalue companyname __typename  } companyStatistics(symbol: $symbol) { symbol ttmtype marketcap sharesoutstanding bv beta eps dilutedeps pe pb dividendyield totalrevenue profit asset roe roa npl financialleverage __typename  }}"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiCompanyInfo>(requestUrl, new StringContent(JsonSerializer.Serialize(mainInfoQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<SsiLeadership?> DownloadLeadershipAsync(string symbol)
        {
            var requestUrl = "https://finfo-iboard.ssi.com.vn/graphql";
            var leaderShipsQuery = new
            {
                operationName = "leaderships",
                variables = new
                {
                    symbol,
                    size = 1000,
                    offset = 1
                },
                query = "query leaderships($symbol: String!, $size: Int, $offset: Int, $order: String, $orderBy: String) { leaderships(symbol: $symbol, size: $size, offset: $offset, order: $order, orderBy: $orderBy) { datas { symbol fullname positionname positionlevel __typename } __typename }}"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiLeadership>(requestUrl, new StringContent(JsonSerializer.Serialize(leaderShipsQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<SsiCapitalAndDividend?> DownloadCapitalAndDividendAsync(string symbol)
        {
            var requestUrl = "https://finfo-iboard.ssi.com.vn/graphql";
            var capitalAndDividendQuery = new
            {
                operationName = "capAndDividend",
                variables = new
                {
                    symbol
                },
                query = "query capAndDividend($symbol: String!) {capAndDividend(symbol: $symbol)}"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiCapitalAndDividend>(requestUrl, new StringContent(JsonSerializer.Serialize(capitalAndDividendQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<SsiStockPriceHistory?> DownloadStockPricesAsync(string symbol, int size = 10000)
        {
            var requestUrl = "https://finfo-iboard.ssi.com.vn/graphql";
            var stockPriceSendQuery = new
            {
                operationName = "stockPrice",
                variables = new
                {
                    symbol,
                    offset = 1,
                    size,
                    fromDate = DateTime.Now.AddDays(-size).ToString("dd/MM/yyyy"),
                    toDate = DateTime.Now.ToString("dd/MM/yyyy"),
                },
                query = "query stockPrice($symbol: String!, $size: Int, $offset: Int, $fromDate: String, $toDate: String) {\n  stockPrice(\n    symbol: $symbol\n    size: $size\n    offset: $offset\n    fromDate: $fromDate\n    toDate: $toDate\n  )\n}\n"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiStockPriceHistory>(requestUrl, new StringContent(JsonSerializer.Serialize(stockPriceSendQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<SsiCorporateAction?> DownloadCorporateActionAsync(string symbol, int size = 10000)
        {
            var requestUrl = "https://finfo-iboard.ssi.com.vn/graphql";
            var corporateActionQuery = new
            {
                operationName = "corporateActions",
                variables = new
                {
                    symbol,
                    size,
                    offset = 1
                },
                query = "query corporateActions($symbol: String, $size: Int, $offset: Int, $order: String, $orderBy: String, $fromDate: String, $toDate: String, $eventcode: String, $datetype: String) {corporateActions(symbol: $symbol, size: $size, offset: $offset, order: $order, orderBy: $orderBy, fromDate: $fromDate, toDate: $toDate, eventcode: $eventcode, datetype: $datetype)}"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiCorporateAction>(requestUrl, new StringContent(JsonSerializer.Serialize(corporateActionQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<FiintradingEvaluate?> DownloadFiinStockEvaluatesAsync(string symbol)
        {
            var requestUrl = $"https://fundamental.fiintrade.vn/Snapshot/GetCompanyScore?language=vi&OrganCode={symbol}";
            var result = await _fiinHttpClient.GetJsonAsync<FiintradingEvaluate>(requestUrl);
            return result;
        }

        public virtual async Task<VndRecommendations?> DownloadStockRecommendationsAsync(string symbol)
        {
            var startDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");
            var requestUrl = $"https://finfo-api.vndirect.com.vn/v4/recommendations?q=code:{symbol}~reportDate:gte:{startDate}&size=100&sort=reportDate:DESC";
            var result = await _vndHttpClient.GetJsonAsync<VndRecommendations>(requestUrl);
            return result;
        }

        public virtual async Task<VndStockScorings?> DownloadVndStockScoringsAsync(string symbol)
        {
            var startDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            var requestUrl = $"https://finfo-api.vndirect.com.vn/v4/scorings/latest?order=fiscalDate&where=code:{symbol}~locale:VN~fiscalDate:lte:{startDate}&filter=criteriaCode:100000,101000,102000,103000,104000,105000,106000,107000";
            var result = await _vndHttpClient.GetJsonAsync<VndStockScorings>(requestUrl);
            return result;
        }

        public virtual async Task<SsiFinancialIndicator?> DownloadFinancialIndicatorAsync(string symbol)
        {
            var requestUrl = "https://finfo-iboard.ssi.com.vn/graphql";
            var financialIndicatorQuery = new
            {
                operationName = "financialIndicator",
                variables = new
                {
                    symbol,
                    size = 1000
                },
                query = "query financialIndicator($symbol: String!, $yearReport: String, $lengthReport: String, $size: Int, $offset: Int) {financialIndicator(symbol: $symbol, yearReport: $yearReport, lengthReport: $lengthReport, size: $size, offset: $offset)}"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiFinancialIndicator>(requestUrl, new StringContent(JsonSerializer.Serialize(financialIndicatorQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<SsiTransaction?> DownloadTransactionAsync(string stockNo)
        {
            var requestUrl = "https://wgateway-iboard.ssi.com.vn/graphql";
            var stockPriceSendQuery = new
            {
                operationName = "leTables",
                variables = new
                {
                    stockNo
                },
                query = "query leTables($stockNo: String) {\n  leTables(stockNo: $stockNo) {\n    stockNo\n    price\n    vol\n    accumulatedVol\n    time\n    ref\n    side\n    priceChange\n    priceChangePercent\n    changeType\n    __typename\n  }\n  stockRealtime(stockNo: $stockNo) {\n    stockNo\n    ceiling\n    floor\n    refPrice\n    stockSymbol\n    __typename\n  }\n}\n"
            };
            return await _ssiHttpClient.PostJsonAsync<SsiTransaction>(requestUrl, new StringContent(JsonSerializer.Serialize(stockPriceSendQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<VndChartPrice?> DownloadVndChartPricesRealTimeAsync(string symbol, string type = "D")
        {
            var toTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            var fromTime = DateTimeOffset.Now.AddDays(-5).ToUnixTimeSeconds();
            var requestUrl = $"https://dchart-api.vndirect.com.vn/dchart/history?resolution={type}&symbol={symbol}&from={fromTime}&to={toTime}";
            var vndChartPrice = await _vndHttpClient.GetJsonAsync<VndChartPrice>(requestUrl);
            return vndChartPrice;
        }

        public virtual async Task<SsiChartPrice?> DownloadSsiChartPricesRealTimeAsync(string symbol, string type = "D")
        {
            var toTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            var fromTime = DateTimeOffset.Now.AddDays(-5).ToUnixTimeSeconds();
            var requestUrl = $"https://iboard.ssi.com.vn/dchart/api/history?resolution={type}&symbol={symbol}&from={fromTime}&to={toTime}";
            return await _ssiHttpClient.GetJsonAsync<SsiChartPrice>(requestUrl);
        }

        public virtual async Task<VndChartPrice?> DownloadVpsChartPricesRealTimeAsync(string symbol, string type = "D")
        {
            var toTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            var fromTime = DateTimeOffset.Now.AddDays(-5).ToUnixTimeSeconds();
            var requestUrl = $"https://histdatafeed.vps.com.vn/tradingview/history?resolution={type}&symbol={symbol}&from={fromTime}&to={toTime}";
            return await _vpsHttpClient.GetJsonAsync<VndChartPrice>(requestUrl);
        }

        public virtual async Task<List<SsiChartPrice>> DownloadSsiChartPricesAsync(string symbol, long configTime, string type = "D")
        {
            var result = new List<SsiChartPrice>();
            var startTime = DateTimeOffset.FromUnixTimeSeconds(configTime);
            for (int i = startTime.Year; i <= (DateTimeOffset.Now.Year + 1); i++)
            {
                await Task.Delay(100);
                var fromTime = startTime.ToUnixTimeSeconds();
                var toTime = startTime.AddYears(1).AddDays(1).ToUnixTimeSeconds();
                var requestUrl = $"https://iboard.ssi.com.vn/dchart/api/history?resolution={type}&symbol={symbol}&from={fromTime}&to={toTime}";
                var ssiIndexPrices = await _ssiHttpClient.GetJsonAsync<SsiChartPrice>(requestUrl);
                if (ssiIndexPrices is not null)
                {
                    result.Add(ssiIndexPrices);
                }
                startTime = startTime.AddYears(1);
            }
            return result;
        }

        public virtual async Task<List<VndChartPrice>> DownloadVndChartPricesAsync(string symbol, long configTime, string type = "D")
        {
            var result = new List<VndChartPrice>();
            var startTime = DateTimeOffset.FromUnixTimeSeconds(configTime);
            for (int i = startTime.Year; i <= (DateTimeOffset.Now.Year + 1); i++)
            {
                await Task.Delay(100);
                var fromTime = startTime.ToUnixTimeSeconds();
                var toTime = startTime.AddYears(1).AddDays(1).ToUnixTimeSeconds();
                var requestUrl = $"https://dchart-api.vndirect.com.vn/dchart/history?resolution={type}&symbol={symbol}&from={fromTime}&to={toTime}";
                var ssiIndexPrices = await _vndHttpClient.GetJsonAsync<VndChartPrice>(requestUrl);
                if (ssiIndexPrices is not null)
                {
                    result.Add(ssiIndexPrices);
                }
                startTime = startTime.AddYears(1);
            }
            return result;
        }

        public virtual async Task<List<BankInterestRateObject>> DownloadBankInterestRateAsync(string periods = "3,6,12,24")
        {
            var result = new List<BankInterestRateObject>();
            var lengthArray = periods.Split(',');
            foreach (var length in lengthArray)
            {
                await Task.Delay(500);
                var requestUrl = $"https://finfo-api.vndirect.com.vn/v4/macro_interests?q=customerType:PERSONAL~channel:COUNTER~paymentType:MATURITY~unit:MONTHLY~term:{length}";
                var bankInterestRateObject = await _vndHttpClient.GetJsonAsync<BankInterestRateObject>(requestUrl);
                if (bankInterestRateObject is not null)
                {
                    result.Add(bankInterestRateObject);
                }
            }
            return result;
        }

        public virtual async Task<List<MarketDepth>> DownloadMarketInDepthAsync()
        {
            var result = new List<MarketDepth>();
            var requestUrl = $"https://fiin-market.ssi.com.vn/MarketInDepth/GetProspectV2?language=vi&ComGroupCode=VNINDEX";
            var jsonString = await _ssiHttpClient.GetJsonAsync(requestUrl);
            if (!string.IsNullOrEmpty(jsonString))
            {
                var jsonDocument = JsonDocument.Parse(jsonString);
                var vnRealtime = jsonDocument.RootElement.GetProperty("items")[0].GetProperty("series").Deserialize<VnIndexRealtime>();
                if (vnRealtime is not null)
                {
                    result.Add(new MarketDepth()
                    {
                        WorldIndexCode = "VNINDEXREALTIME",
                        IndexValue = vnRealtime.IndexValue,
                        IndexChange = vnRealtime.IndexChange,
                        PercentIndexChange = vnRealtime.PercentIndexChange,
                        TradingDate = vnRealtime.TradingDate,
                    });
                }
                var vnMarketDepth = jsonDocument.RootElement.GetProperty("items")[0].GetProperty("heatMap").GetProperty("heatmaps")[0].GetProperty("vnMarket").Deserialize<VnMarketDepth>();
                if (vnMarketDepth is not null)
                {
                    result.Add(new MarketDepth()
                    {
                        WorldIndexCode = "VNINDEX",
                        IndexValue = vnMarketDepth.VnIndex.IndexValue,
                        IndexChange = vnMarketDepth.VnIndex.IndexChange,
                        PercentIndexChange = vnMarketDepth.VnIndex.PercentIndexChange,
                        TradingDate = vnMarketDepth.VnIndex.TradingDate,
                    });
                    result.Add(new MarketDepth()
                    {
                        WorldIndexCode = "VN30",
                        IndexValue = vnMarketDepth.Vn30.IndexValue,
                        IndexChange = vnMarketDepth.Vn30.IndexChange,
                        PercentIndexChange = vnMarketDepth.Vn30.PercentIndexChange,
                        TradingDate = vnMarketDepth.Vn30.TradingDate,
                    });
                    result.Add(new MarketDepth()
                    {
                        WorldIndexCode = "HNXINDEX",
                        IndexValue = vnMarketDepth.HnxIndex.IndexValue,
                        IndexChange = vnMarketDepth.HnxIndex.IndexChange,
                        PercentIndexChange = vnMarketDepth.HnxIndex.PercentIndexChange,
                        TradingDate = vnMarketDepth.HnxIndex.TradingDate,
                    });
                    result.Add(new MarketDepth()
                    {
                        WorldIndexCode = "HNX30",
                        IndexValue = vnMarketDepth.Hnx30.IndexValue,
                        IndexChange = vnMarketDepth.Hnx30.IndexChange,
                        PercentIndexChange = vnMarketDepth.Hnx30.PercentIndexChange,
                        TradingDate = vnMarketDepth.Hnx30.TradingDate,
                    });
                }
                var marketDepths = jsonDocument.RootElement.GetProperty("items")[0].GetProperty("heatMap").GetProperty("heatmaps")[0].GetProperty("usMarket").Deserialize<List<MarketDepth>>();
                if (marketDepths is not null)
                {
                    result.AddRange(marketDepths);
                }
                marketDepths = jsonDocument.RootElement.GetProperty("items")[0].GetProperty("heatMap").GetProperty("heatmaps")[0].GetProperty("europMarket").Deserialize<List<MarketDepth>>();
                if (marketDepths is not null)
                {
                    result.AddRange(marketDepths);
                }
                marketDepths = jsonDocument.RootElement.GetProperty("items")[0].GetProperty("heatMap").GetProperty("heatmaps")[0].GetProperty("asianMarket").Deserialize<List<MarketDepth>>();
                if (marketDepths is not null)
                {
                    result.AddRange(marketDepths);
                }
            }
            return result;
        }

        public virtual async Task<Dictionary<string, List<FinIndexValuation>>> DownloadFiinValuationAsync(string indexs = "VNINDEX,VN30,HNX30")
        {
            var result = new Dictionary<string, List<FinIndexValuation>>();
            foreach (var index in indexs.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var requestUrl = $"https://fiin-market.ssi.com.vn/MarketInDepth/GetValuationSeriesV2?language=vi&Code={index}&TimeRange=ThreeMonths&FromDate=&ToDate=";
                var jsonString = await _ssiHttpClient.GetJsonAsync(requestUrl);
                if (!string.IsNullOrEmpty(jsonString))
                {
                    var jsonDocument = JsonDocument.Parse(jsonString);
                    var indexValuations = jsonDocument.RootElement.GetProperty("items").Deserialize<List<FinIndexValuation>>();
                    result.Add(index, indexValuations ?? new List<FinIndexValuation>());
                }
                await Task.Delay(500);
            }
            return result;
        }
    }
}
