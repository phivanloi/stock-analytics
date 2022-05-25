using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.WebDashboard.Models;
using System.Text;
using System.Text.Json;

namespace Pl.Sas.WebDashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly StockViewService _stockViewService;
        private readonly IZipHelper _zipHelper;
        private readonly IStockData _stockData;
        private readonly IFollowStockData _followStockData;
        private readonly IKeyValueData _keyValueData;
        private readonly IAnalyticsResultData _analyticsResultData;
        private readonly IStockPriceData _stockPriceData;
        private readonly ICompanyData _companyData;
        private readonly ITradingResultData _tradingResultData;
        private readonly IIndustryData _industryData;
        private readonly ICorporateActionData _corporateActionData;

        public HomeController(
            ICorporateActionData corporateActionData,
            IIndustryData industryData,
            ITradingResultData tradingResultData,
            ICompanyData companyData,
            IStockPriceData stockPriceData,
            IAnalyticsResultData analyticsResultData,
            IKeyValueData keyValueData,
            IFollowStockData followStockData,
            IStockData stockData,
            StockViewService stockViewService,
            IZipHelper zipHelper)
        {
            _corporateActionData = corporateActionData;
            _analyticsResultData = analyticsResultData;
            _keyValueData = keyValueData;
            _stockData = stockData;
            _stockViewService = stockViewService;
            _zipHelper = zipHelper;
            _followStockData = followStockData;
            _stockPriceData = stockPriceData;
            _companyData = companyData;
            _tradingResultData = tradingResultData;
            _industryData = industryData;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var userId = HttpContext.GetUserId();
            var model = new MarketViewModel()
            {
                Exchanges = await _stockData.GetExchanges(),
                IndustryCodes = await _stockViewService.CacheGetIndustriesAsync(),
                UserHasFollowStock = await _followStockData.IsUserHasFollowAsync(userId)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarketListAsync([FromBody] MarketSearchModel? marketSearchModel = null)
        {
            var userId = HttpContext.GetUserId();
            var userFollowStocksTask = _followStockData.FindAllAsync(userId);
            var bankInterestRate12Task = _keyValueData.GetAsync(Constants.BankInterestRate12Key);
            var followSymbols = (await userFollowStocksTask).Select(q => q.Symbol).ToList();
            var model = new MarketListViewModel()
            {
                StockViews = _stockViewService.GetMarketStocksView(marketSearchModel?.Principle ?? 1, marketSearchModel?.Exchange, marketSearchModel?.IndustryCode, marketSearchModel?.Symbol, marketSearchModel?.Ordinal, marketSearchModel?.Zone, followSymbols),
                UserFollowSymbols = followSymbols,
                Principle = marketSearchModel?.Principle ?? 1,
                BankInterestRate12 = (await bankInterestRate12Task)?.GetValue<float>() ?? 6.8f,
            };
            return PartialView(model);
        }

        public async Task<IActionResult> ToggleFollowAsync(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return Json(new { status = 0, message = "Dữ liệu không hợp lệ." });
            }

            var userId = HttpContext.GetUserId();
            var followStock = await _followStockData.FindAsync(userId, symbol);
            if (followStock != null)
            {
                var deleteResult = await _followStockData.DeleteAsync(followStock.Id);
                return Json(new { status = deleteResult ? 1 : 0, message = deleteResult ? $"Bỏ theo dõi mã {symbol} thành công." : $"Bỏ theo dõi mã {symbol} không thành công." });
            }
            else
            {
                var insertResult = await _followStockData.InsertAsync(new FollowStock
                {
                    UserId = userId,
                    Symbol = symbol
                });
                return Json(new { status = insertResult ? 1 : 0, message = insertResult ? $"Theo dõi mã {symbol} thành công." : $"Theo dõi mã {symbol} không thành công." });
            }
        }

        public async Task<IActionResult> StockDetailsAsync(string symbol)
        {
            var stockTask = _stockData.GetByCodeAsync(symbol);
            var analyticsResultTask = _analyticsResultData.FindAsync(symbol);
            var allStockPricesTask = _stockPriceData.FindAllAsync(symbol);
            var companyTask = _companyData.GetByCodeAsync(symbol);
            var tradingResultsTask = _tradingResultData.FindAllAsync(symbol);
            var stock = await stockTask;
            if (stock is null)
            {
                return Content($"<p style=\"color: #fff; text-align: center;\">Không tìm thấy mã: {symbol}.</p>", "text/html");
            }
            var analyticsResult = await analyticsResultTask;
            if (analyticsResult is null)
            {
                return Content($"<p style=\"color: #fff; text-align: center;\">Mã {symbol} không có thông tin phân tích.</p>", "text/html");
            }

            var model = new StockDetailsModel()
            {
                Symbol = symbol,
                Details = stock,
                StockPrices = await allStockPricesTask,
                AnalyticsResultInfo = analyticsResult,
                CompanyInfo = await companyTask,
            };

            foreach (var tradingResult in await tradingResultsTask)
            {
                if (!model.TradingResults.ContainsKey(tradingResult.Principle))
                {
                    model.TradingResults.Add(tradingResult.Principle, new()
                    {
                        TradingExplainNotes = tradingResult.TradingNotes is not null ? JsonSerializer.Deserialize<List<KeyValuePair<int, string>>>(_zipHelper.UnZipByte(tradingResult.TradingNotes)) : new List<KeyValuePair<int, string>>(),
                        Capital = tradingResult.Capital,
                        Profit = tradingResult.Profit,
                        ProfitPercent = tradingResult.ProfitPercent,
                        TotalTax = tradingResult.TotalTax
                    });
                }
            }

            if (analyticsResult.MarketNotes is not null)
            {
                model.MacroeconomicsNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.MarketNotes));
            }
            if (analyticsResult.CompanyValueNotes is not null)
            {
                model.CompanyValueNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.CompanyValueNotes));
            }
            if (analyticsResult.CompanyGrowthNotes is not null)
            {
                model.CompanyGrowthNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.CompanyGrowthNotes));
            }
            if (analyticsResult.StockNotes is not null)
            {
                model.StockNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.StockNotes));
            }
            if (analyticsResult.FiinNotes is not null)
            {
                model.FiinNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.FiinNotes));
            }
            if (analyticsResult.VndNotes is not null)
            {
                model.VndNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.VndNotes));
            }
            if (analyticsResult.TargetPriceNotes is not null)
            {
                model.TargetPriceNotes = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.TargetPriceNotes));
            }
            if (model.CompanyInfo is not null && model.CompanyInfo.CompanyProfile is not null)
            {
                model.CompanyProfile = Encoding.UTF8.GetString(_zipHelper.UnZipByte(model.CompanyInfo.CompanyProfile));
            }
            return PartialView(model);
        }

        public IActionResult IndexChart()
        {
            return View();
        }

        public async Task<IActionResult> IndustryAsync()
        {
            var model = new IndustryViewModel()
            {
                Industries = (await _industryData.FindAllAsync()).OrderByDescending(q => q.Rank).ThenByDescending(q => q.AutoRank).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> IndustrySaveAsync(string id, int rank)
        {
            var message = "Thay đổi không thành công.";
            var updateResult = false;
            var industry = await _industryData.GetByIdAsync(id);
            if (industry is not null)
            {
                industry.Rank = rank;
                updateResult = await _industryData.UpdateAsync(industry);
                if (updateResult)
                {
                    message = $"Thay đổi lĩnh vực '{industry.Name}' thành công.";
                }
            }
            return Json(new { status = updateResult ? 1 : 0, message });
        }

        public async Task<IActionResult> CorporateActionAsync()
        {
            return View(await _stockData.GetExchanges());
        }

        [HttpPost]
        public async Task<IActionResult> CorporateActionAsync(string? symbol = null, string? evenCode = null, string? exchange = null)
        {
            var corporateActions = await _corporateActionData.FindAllForViewPageAsync(symbol, evenCode?.Split(','), exchange);
            corporateActions = corporateActions.OrderBy(q => q.ExrightDate).ThenByDescending(q => q.PublicDate).ToList();
            var responseList = new List<CorporateActionViewModel>();
            foreach (var item in corporateActions)
            {
                responseList.Add(new CorporateActionViewModel()
                {
                    Id = item.Id,
                    Symbol = item.Symbol,
                    ExrightDate = item.ExrightDate.ToString("dd/MM/yyyy"),
                    RecordDate = item.RecordDate.ToString("dd/MM/yyyy"),
                    IssueDate = item.IssueDate.ToString("dd/MM/yyyy"),
                    PublicDate = item.PublicDate.ToString("dd/MM/yyyy"),
                    EventTitle = item.EventTitle,
                    Exchange = item.Exchange,
                    Content = item.Description is not null ? Encoding.UTF8.GetString(_zipHelper.UnZipByte(item.Description)) : "",
                    EventCode = item.EventCode
                });
            }
            return Json(responseList);
        }
    }
}