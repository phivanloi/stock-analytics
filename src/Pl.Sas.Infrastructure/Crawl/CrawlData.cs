﻿using Pl.Sas.Core.Entities.CrawlObjects;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Helper;
using System.Text;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Crawl
{
    public class CrawlData : ICrawlData
    {
        private readonly HttpClient _httpClient;
        public CrawlData(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("downloader");
        }

        public virtual async Task<SsiAllStock?> DownloadInitialMarketStockAsync()
        {
            var requestUrl = "https://iboard.ssi.com.vn/dchart/api/1.1/defaultAllStocks";
            return await _httpClient.GetJsonAsync<SsiAllStock>(requestUrl);
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
            return await _httpClient.PostJsonAsync<SsiCompanyInfo>(requestUrl, new StringContent(JsonSerializer.Serialize(mainInfoQuery), Encoding.UTF8, "application/json"));
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
            return await _httpClient.PostJsonAsync<SsiLeadership>(requestUrl, new StringContent(JsonSerializer.Serialize(leaderShipsQuery), Encoding.UTF8, "application/json"));
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
            return await _httpClient.PostJsonAsync<SsiCapitalAndDividend>(requestUrl, new StringContent(JsonSerializer.Serialize(capitalAndDividendQuery), Encoding.UTF8, "application/json"));
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
                    size
                },
                query = "query stockPrice($symbol: String!, $size: Int, $offset: Int, $fromDate: String, $toDate: String) { stockPrice(symbol: $symbol, size: $size, offset: $offset, fromDate: $fromDate, toDate: $toDate) }"
            };
            var stockPriceSendContent = new StringContent(JsonSerializer.Serialize(stockPriceSendQuery), Encoding.UTF8, "application/json");
            return await _httpClient.PostJsonAsync<SsiStockPriceHistory>(requestUrl, new StringContent(JsonSerializer.Serialize(stockPriceSendContent), Encoding.UTF8, "application/json"));
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
            return await _httpClient.PostJsonAsync<SsiCorporateAction>(requestUrl, new StringContent(JsonSerializer.Serialize(corporateActionQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<FiintradingEvaluate?> DownloadFiinStockEvaluateAsync(string symbol)
        {
            var requestUrl = $"https://fundamental.fiintrade.vn/Snapshot/GetCompanyScore?language=vi&OrganCode={symbol}";
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://fiintrade.vn");
            var result = await _httpClient.GetJsonAsync<FiintradingEvaluate>(requestUrl);
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Remove("Origin");
            return result;
        }

        public virtual async Task<VndRecommendations?> DownloadStockRecommendationsAsync(string symbol)
        {
            var startDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");
            var requestUrl = $"https://finfo-api.vndirect.com.vn/v4/recommendations?q=code:{symbol}~reportDate:gte:{startDate}&size=100&sort=reportDate:DESC";
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://dstock.vndirect.com.vn");
            var result = await _httpClient.GetJsonAsync<VndRecommendations>(requestUrl);
            _httpClient.DefaultRequestHeaders.Remove("Origin");
            return result;
        }

        public virtual async Task<VndStockScorings?> DownloadVndStockScoringsAsync(string symbol)
        {
            var startDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            var requestUrl = $"https://finfo-api.vndirect.com.vn/v4/scorings/latest?order=fiscalDate&where=code:{symbol}~locale:VN~fiscalDate:lte:{startDate}&filter=criteriaCode:100000,101000,102000,103000,104000,105000,106000,107000";
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://dstock.vndirect.com.vn");
            var result = await _httpClient.GetJsonAsync<VndStockScorings>(requestUrl);
            _httpClient.DefaultRequestHeaders.Remove("Origin");
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
            return await _httpClient.PostJsonAsync<SsiFinancialIndicator>(requestUrl, new StringContent(JsonSerializer.Serialize(financialIndicatorQuery), Encoding.UTF8, "application/json"));
        }
    }
}
