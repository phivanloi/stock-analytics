using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Helper;

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
    }
}
