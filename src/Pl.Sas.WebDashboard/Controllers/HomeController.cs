using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.WebDashboard.Models;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Identity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pl.Sas.Core.Services;

namespace Pl.Sas.WebDashboard.Controllers
{
    [Authorize(Roles = PermissionConstants.CmsDashbroad)]
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
            var userId = HttpContext.GetUserId();
            var lastStockPrice = _marketData.GetLastStockPriceAsync(symbol);
            var stockTask = _marketData.CacheGetStockByCodeAsync(symbol);
            var analyticsResultTask = _analyticsData.CacheGetAnalyticsResultAsync(symbol, datePath);
            var userStockNoteTask = _userStockNoteData.FindAsync(userId, symbol);
            var allStockPricesTask = _stockViewService.CacheGetStockPricesForDetailPageAsync(symbol);
            var companyTask = _stockViewService.CacheGetCompanyByCodeAsync(symbol);
            var tradingResultsTask = _tradingResultData.FindAllAsync(symbol, datePath);
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
                Note = (await userStockNoteTask)?.Note ?? ""
            };

            foreach (var tradingResult in await tradingResultsTask)
            {
                if (!model.TradingResults.ContainsKey(tradingResult.Principle))
                {
                    model.TradingResults.Add(tradingResult.Principle, new()
                    {
                        TradingExplainNotes = JsonSerializer.Deserialize<List<KeyValuePair<int, string>>>(_zipHelper.UnZipByte(tradingResult.ZipExplainNotes)),
                        Capital = tradingResult.Capital,
                        Profit = tradingResult.Profit,
                        ProfitPercent = tradingResult.ProfitPercent,
                        TotalTax = tradingResult.TotalTax
                    });
                }
            }

            if (analyticsResult.MacroeconomicsNote is not null)
            {
                model.MacroeconomicsNote = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.MacroeconomicsNote));
            }
            if (analyticsResult.CompanyValueNote is not null)
            {
                model.CompanyValueNote = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.CompanyValueNote));
            }
            if (analyticsResult.CompanyGrowthNote is not null)
            {
                model.CompanyGrowthNote = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.CompanyGrowthNote));
            }
            if (analyticsResult.StockNote is not null)
            {
                model.StockNote = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.StockNote));
            }
            if (analyticsResult.FiinNote is not null)
            {
                model.FiinNote = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.FiinNote));
            }
            if (analyticsResult.VndNote is not null)
            {
                model.VndNote = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.VndNote));
            }
            if (analyticsResult.TargetPriceNotes is not null)
            {
                model.TargetPriceNotes = JsonSerializer.Deserialize<List<AnalyticsMessage>>(_zipHelper.UnZipByte(analyticsResult.TargetPriceNotes));
            }
            if (model.CompanyInfo is not null && model.CompanyInfo.CompanyProfileZip is not null)
            {
                model.CompanyProfile = Encoding.UTF8.GetString(_zipHelper.UnZipByte(model.CompanyInfo.CompanyProfileZip));
            }
            return PartialView(model);
        }

        public async Task<IActionResult> UserStockDetailsSaveAsync([FromBody] UserStockDetailsSaveModel userStockDetailsSaveModel)
        {
            if (string.IsNullOrEmpty(userStockDetailsSaveModel.Symbol))
            {
                return Json(new { status = 0, message = "Dữ liệu không hợp lệ." });
            }

            if (string.IsNullOrEmpty(userStockDetailsSaveModel.Note))
            {
                return Json(new { status = 1, message = "Ghi thành công." });
            }

            var userId = HttpContext.GetUserId();
            var userStockNote = await _userStockNoteData.FindAsync(userId, userStockDetailsSaveModel.Symbol);

            string message;
            int status;
            if (userStockNote is null)
            {
                userStockNote = new()
                {
                    Symbol = userStockDetailsSaveModel.Symbol,
                    Note = userStockDetailsSaveModel.Note,
                    UserId = userId
                };
                var insertResult = await _userStockNoteData.InsertAsync(userStockNote);
                message = insertResult ? "Thêm mới thành công." : "Thêm mới không thành công.";
                status = insertResult ? 1 : 0;
            }
            else
            {
                userStockNote.Note = userStockDetailsSaveModel.Note;
                var updateResult = await _userStockNoteData.UpdateAsync(userStockNote);
                message = updateResult ? "Thêm mới thành công." : "Thêm mới không thành công.";
                status = updateResult ? 1 : 0;
            }
            return Json(new { status, message });
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
            return View(await _stockViewService.CacheGetExchangesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CorporateActionAsync(string symbol, string evenCode, string exchange)
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
                    Content = Encoding.UTF8.GetString(_zipHelper.UnZipByte(item.ZipDescription)),
                    EventCode = item.EventCode
                });
            }
            return Json(responseList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}