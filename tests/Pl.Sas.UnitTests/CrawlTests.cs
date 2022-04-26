using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class CrawlTests
    {
        [Fact]
        public async Task DownloadInitialMarketStockTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<ICrawlData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var ssiAllStock = await crawlData.DownloadInitialMarketStockAsync();
            Assert.True(ssiAllStock != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task DownloadCompanyInfoTestAsync()
        {
            var services = ConfigureServices.GetConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var crawlData = serviceProvider.GetService<ICrawlData>() ?? throw new Exception("Can't get ICrawlData");
            var hostedService = serviceProvider.GetService<IHostedService>() as LoggingQueuedHostedService ?? throw new Exception("Can't get LoggingQueuedHostedService");
            await hostedService.StartAsync(CancellationToken.None);

            var ssiCompanyInfo = await crawlData.DownloadCompanyInfoAsync("TVC");
            Assert.True(ssiCompanyInfo != null);

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task GetApiCompanyInfoTest()
        {
            var url = "https://finfo-iboard.ssi.com.vn/graphql";
            var postObject = new
            {
                operationName = "companyProfile",
                variables = new
                {
                    symbol = "DBC",
                    language = "vn"
                },
                query = "query companyProfile($symbol: String!, $language: String) { companyProfile(symbol: $symbol, language: $language) { symbol subsectorcode industryname supersector sector subsector foundingdate chartercapital numberofemployee banknumberofbranch companyprofile listingdate exchange firstprice issueshare listedvalue companyname __typename  } companyStatistics(symbol: $symbol) { symbol ttmtype marketcap sharesoutstanding bv beta eps dilutedeps pe pb dividendyield totalrevenue profit asset roe roa npl financialleverage __typename  }}"
            };
            var content = new StringContent(JsonSerializer.Serialize(postObject), Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
            var result = await client.PostAsync(url, content);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.NotEmpty(contentResult);
        }

        [Fact]
        public async Task GetApiCapitalAndDividendInfoTest()
        {
            var url = "https://finfo-iboard.ssi.com.vn/graphql";
            var capitalAndDividendQuery = new
            {
                operationName = "capAndDividend",
                variables = new
                {
                    symbol = "STB"
                },
                query = "query capAndDividend($symbol: String!) {capAndDividend(symbol: $symbol)}"
            };
            var content = new StringContent(JsonSerializer.Serialize(capitalAndDividendQuery), Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
            var result = await client.PostAsync(url, content);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.NotEmpty(contentResult);
        }

        [Fact]
        public async Task GetMatchCommandTest()
        {
            var url = "https://finfo-iboard.ssi.com.vn/graphql";
            var sendObject = new
            {
                operationName = "stockPrice",
                variables = new
                {
                    symbol = "VIC",
                    size = 1,
                    offset = 1
                },
                query = "query stockPrice($symbol: String!, $size: Int, $offset: Int, $fromDate: String, $toDate: String) {stockPrice(symbol: $symbol, size: $size, offset: $offset, fromDate: $fromDate, toDate: $toDate)}"
            };
            var content = new StringContent(JsonSerializer.Serialize(sendObject), Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
            var result = await client.PostAsync(url, content);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.NotEmpty(contentResult);
        }

        [Fact]
        public async Task GetFiinNewsTest()
        {
            var url = "https://market.fiintrade.vn/WatchList/GetWatchListNews?language=vi&Page=1&PageSize=100&SourceCode=&WatchListId=17&WatchListType=Sector";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer");
            client.DefaultRequestHeaders.Add("Origin", "https://fiintrade.vn");
            var result = await client.GetAsync(url);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.NotEmpty(contentResult);
        }

        [Fact]
        public async Task GetIndexHistoryTest()
        {
            var symbol = "VNINDEX";
            var startTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromMilliseconds(0));
            var contents = new List<string>();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
            for (int i = 2000; i <= (DateTime.Now.Year + 1); i++)
            {
                var fromTime = startTime.ToUnixTimeSeconds();
                var toTime = startTime.AddYears(1).AddDays(1).ToUnixTimeSeconds();
                var callUrl = $"https://dchart-api.vndirect.com.vn/dchart/history?resolution=D&symbol={symbol}&from={fromTime}&to={toTime}";
                var result = await client.GetAsync(callUrl);
                contents.Add(await result.Content.ReadAsStringAsync());
                startTime = startTime.AddYears(1);
            }
            Assert.True(contents.Count > 0);
        }

        [Fact]
        public async Task GetFiinEvaluateTest()
        {
            var url = "https://fundamental.fiintrade.vn/Snapshot/GetCompanyScore?language=vi&OrganCode=TIG";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer");
            client.DefaultRequestHeaders.Add("Origin", "https://fiintrade.vn");
            var result = await client.GetAsync(url);
            var contentResult = await result.Content.ReadAsStringAsync();
            Assert.NotEmpty(contentResult);
        }
    }
}