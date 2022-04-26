using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Helper;
using System.Text;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Crawl
{
    public class CrawlData : ICrawlData
    {
        private readonly IHttpHelper httpHelper;
        public CrawlData(IHttpClientFactory httpClientFactory)
        {
            httpHelper = new HttpHelper(httpClientFactory.CreateClient("downloader"));
        }

        public virtual async Task<SsiAllStock?> DownloadInitialMarketStockAsync()
        {
            var requestUrl = "https://iboard.ssi.com.vn/dchart/api/1.1/defaultAllStocks";
            return await httpHelper.GetJsonAsync<SsiAllStock>(requestUrl);
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
            return await httpHelper.PostJsonAsync<SsiCompanyInfo>(requestUrl, new StringContent(JsonSerializer.Serialize(mainInfoQuery), Encoding.UTF8, "application/json"));
        }

        public virtual async Task<SsiLeadership?> DownloadLeadershipFromSsiAsync(string symbol)
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
            return await httpHelper.PostJsonAsync<SsiLeadership>(requestUrl, new StringContent(JsonSerializer.Serialize(leaderShipsQuery), Encoding.UTF8, "application/json"));
        }
    }
}
