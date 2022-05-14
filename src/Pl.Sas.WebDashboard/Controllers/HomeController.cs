using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.WebDashboard.Models;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using System.Text;
using System.Text.Json;
using Pl.Sas.Core.Services;

namespace Pl.Sas.WebDashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly StockViewService _stockViewService;
        private readonly IIdentityData _identityData;
        private readonly IZipHelper _zipHelper;
        private readonly IMarketData _marketData;
        private readonly IAnalyticsData _analyticsData;
        private readonly ISystemData _systemData;

        public HomeController(
            StockViewService stockViewService,
            IIdentityData identityData,
            IMarketData marketData,
            IAnalyticsData analyticsData,
            ISystemData systemData,
            IZipHelper zipHelper)
        {
            _systemData = systemData;
            _analyticsData = analyticsData;
            _marketData = marketData;
            _identityData = identityData;
            _stockViewService = stockViewService;
            _zipHelper = zipHelper;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var userId = HttpContext.GetUserId();
            var model = new MarketViewModel()
            {
                Exchanges = await _marketData.CacheGetExchangesAsync(),
                IndustryCodes = await _stockViewService.CacheGetIndustriesAsync(),
                UserHasFollowStock = await _identityData.IsUserHasFollowAsync(userId)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarketListAsync([FromBody] MarketSearchModel? marketSearchModel = null)
        {
            var userId = HttpContext.GetUserId();
            var userFollowStocksTask = _identityData.GetFollowSymbols(userId);
            var bankInterestRate12Task = _systemData.GetKeyValueAsync(Constants.BankInterestRate12Key);
            var followSymbols = await userFollowStocksTask;
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
            var check = await _identityData.ToggleFollow(userId, symbol);
            return Json(new { status = check ? 1 : 0, message = check ? $"Thay đổi {symbol} thành công." : $"Thay đổi {symbol} không thành công." });
        }

        public async Task<IActionResult> StockDetailsAsync(string symbol)
        {
            var stockTask = _marketData.CacheGetStockByCodeAsync(symbol);
            var analyticsResultTask = _analyticsData.CacheGetAnalyticsResultAsync(symbol);
            var allStockPricesTask = _marketData.CacheGetAllStockPricesAsync(symbol);
            var companyTask = _marketData.CacheGetCompanyByCodeAsync(symbol);
            var tradingResultsTask = _analyticsData.CacheGetTradingResultAsync(symbol);
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
            if (analyticsResult.VndNote is not null)
            {
                model.VndNote = JsonSerializer.Deserialize<List<AnalyticsNote>>(_zipHelper.UnZipByte(analyticsResult.VndNote));
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
            var industries = await _marketData.GetIndustriesAsync();
            var industryAnalytics = await _analyticsData.GetIndustryAnalyticsAsync();
            var model = new List<IndustryViewModel>();
            foreach (var item in industries)
            {
                var analiticsItem = industryAnalytics.FirstOrDefault(q => q.Code == item.Code);
                model.Add(new IndustryViewModel()
                {
                    Code = item.Code,
                    Name = item.Name,
                    ManualScore = analiticsItem?.ManualScore ?? 0,
                    Score = analiticsItem?.Score ?? 0,
                });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> IndustrySaveAsync(string code, float rank)
        {
            var message = "Thay đổi không thành công.";
            var updateResult = false;
            var check = await _analyticsData.SaveManualScoreAsync(code, rank);
            if (check)
            {
                message = $"Thay đổi điểm đánh giá thành công.";
            }
            return Json(new { status = updateResult ? 1 : 0, message });
        }

        public async Task<IActionResult> CorporateActionAsync()
        {
            return View(await _marketData.CacheGetExchangesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CorporateActionAsync(string symbol, string evenCode, string exchange)
        {
            var corporateActions = await _marketData.GetCorporateActionsAsync(symbol, exchange, evenCode?.Split(','));
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