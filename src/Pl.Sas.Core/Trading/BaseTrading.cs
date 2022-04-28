using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;
using Pl.Sas.Core.Interfaces;
using System.Text.Json;

namespace Pl.Sas.Core.Trading
{
    public class BaseTrading
    {
        public const float BuyTax = 0.25f / 100;
        public const float SellTax = 0.35f / 100;
        public const int batch = 100;
        public static readonly int[] Stochastics = new int[] { 12, 14, 16 };
        public static readonly int[] StochasticCheck = new int[] { 0, 20, 30, 50, 70, 80 };
        public static readonly int[] Emas = new int[] { 1, 3, 5, 8, 10, 12, 16, 20, 25, 35 };

        /// <summary>
        /// Hàm sử dụng để bán cổ phiếu và trả ra số tiền thu được sau khi đã trừ thuế
        /// </summary>
        /// <param name="stockCount">Số cổ phiếu</param>
        /// <param name="stockPrice">Giá cổ phiếu lúc bán</param>
        /// <returns>
        /// TotalProfit tổng số tiền thu được
        /// TotalTax tổng số thuế phải trả
        /// </returns>
        public static (float TotalProfit, float TotalTax) Sell(int stockCount, float stockPrice)
        {
            var totalMoney = stockCount * stockPrice;
            var totalTax = totalMoney * SellTax;
            return (totalMoney - totalTax, totalTax);
        }

        /// <summary>
        /// Mua vào một cố phiếu
        /// </summary>
        /// <param name="totalMoney">Tổng số tiền dùng để mua cô phiếu</param>
        /// <param name="stockPrice">Giá cổ phiếu mua vào</param>
        /// <returns>
        /// StockCount số cổ  phiếu mua được, 0 thì là không mua được
        /// ExcessCash số tiền dư còn lại
        /// TotalTax tổng số thuế phải trả
        /// </returns>
        public static (float StockCount, float ExcessCash, float TotalTax) Buy(float totalMoney, float stockPrice)
        {
            if (stockPrice == 0)
            {
                return (0, totalMoney, 0);
            }

            if (true)
            {

            }

            var totalBuyMoney = totalMoney - (totalMoney * tax);
            var buyStockCount = Math.Floor(totalBuyMoney / stockPrice);
            var totalValueStock = buyStockCount * stockPrice;
            var totalTax = totalValueStock * tax;
            var excessCash = totalMoney - (totalValueStock + totalTax);
            return (buyStockCount, excessCash, totalTax);
        }

        /// <summary>
        /// Hàm lấy tỉ lệ biến động giá của các sàn chứng khoán
        /// </summary>
        /// <param name="exchangeName">Tên sàn</param>
        /// <param name="isStartTrading">Có là ngày đầu tiên bắt đầu giao dịch hay không hoặc là ngày đầu tiên sau 25 phiên không giao dịch</param>
        /// <returns></returns>
        public static decimal GetExchangeFluctuationsRate(string exchangeName, bool isStartTrading = false)
        {
            if (isStartTrading)
            {
                return exchangeName switch
                {
                    "HOSE" => 20,
                    "HNX" => 30,
                    "UPCOM" => 40,
                    _ => 20,
                };
            }
            return exchangeName switch
            {
                "HOSE" => 7,
                "HNX" => 10,
                "UPCOM" => 15,
                _ => 7,
            };
        }

        /// <summary>
        /// Build tập hợp các chỉ báo theo lịch sử giá
        /// </summary>
        /// <param name="stockPriceAdjs">Danh sach lịch sử giá đã được điều chỉnh khi chia cổ tức sắp xếp theo phiên mới mất lên đầu</param>
        /// <returns>Dictionary string, IndicatorSet</returns>
        public static Dictionary<string, IndicatorSet> BuildIndicatorSet(List<StockPriceAdj> stockPriceAdjs)
        {
            var indicatorSet = new Dictionary<string, IndicatorSet>();
            var maPeriod = new List<int>();
            for (int p = 1; p <= 50; p++)
            {
                maPeriod.Add(p);
            }
            maPeriod.AddRange(new List<int>() { 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200 });
            var basePeriod = new List<int> { 5, 10, 12, 14, 16, 20 };

            for (int i = stockPriceAdjs.Count - 1; i >= 0; i--)
            {
                var addItem = new IndicatorSet()
                {
                    DatePath = stockPriceAdjs[i].DatePath,
                    TradingDate = stockPriceAdjs[i].TradingDate,
                    Symbol = stockPriceAdjs[i].Symbol,
                    Values = new Dictionary<string, decimal>()
                };
                if (indicatorSet.Count <= 0)
                {
                    foreach (var index in basePeriod)
                    {
                        addItem.Values.Add($"stochastic-{index}", 0);
                        addItem.Values.Add($"rsi-{index}", 0);
                    }
                    foreach (var index in maPeriod)
                    {
                        addItem.Values.Add($"ema-{index}", stockPriceAdjs[i].ClosePrice);
                    }
                }
                else
                {
                    var yesterdaySet = indicatorSet[stockPriceAdjs[i + 1].DatePath];
                    foreach (var index in basePeriod)
                    {
                        if (indicatorSet.Count > index)
                        {
                            var items = stockPriceAdjs.Skip(i).Take(index).ToList();
                            addItem.Values.Add($"stochastic-{index}", Stochastic.CalculateStochastic(items));
                            addItem.Values.Add($"rsi-{index}", RelativeStrengthIndex.CalculateRsi(items));
                        }
                        else
                        {
                            addItem.Values.Add($"stochastic-{index}", 0);
                            addItem.Values.Add($"rsi-{index}", 0);
                        }
                    }
                    foreach (var index in maPeriod)
                    {
                        addItem.Values.Add($"ema-{index}", ExponentialMovingAverage.ExponentialMovingAverageFormula(stockPriceAdjs[i].ClosePrice, yesterdaySet.Values[$"ema-{index}"], index));
                    }
                }
                indicatorSet.Add(stockPriceAdjs[i].DatePath, addItem);
            }
            return indicatorSet;
        }

        /// <summary>
        /// Chuyển lịch sử giả bình thường thành lịch sử giá điều chỉnh dùng cho trading
        /// </summary>
        /// <param name="stockPrices">Lịch sử giá cổ phiếu</param>
        /// <returns>List StockPriceAdj</returns>
        public static List<StockPriceAdj> ConvertStockPricesToStockPriceAdj(IEnumerable<StockPrice> stockPrices)
        {
            GuardClauses.NullOrEmpty(stockPrices, nameof(stockPrices));
            return stockPrices.Select(q => q.ToStockPriceAdj()).ToList();
        }

        /// <summary>
        /// Chuyển lịch sử giao dịch của phiên thành chi tiết các giao dịch
        /// </summary>
        /// <param name="stockTransactions">Danh sách lịch sử giá</param>
        /// <param name="stockPrices">Lịch sử giá dùng để chuyển đổi</param>
        /// <param name="zipHelper"></param>
        /// <returns></returns>
        public static List<TradingStockTransaction> ConvertStockTransactionToTradingStockTransaction(IEnumerable<StockTransaction> stockTransactions, IEnumerable<StockPrice> stockPrices, IZipHelper zipHelper)
        {
            GuardClauses.NullOrEmpty(stockTransactions, nameof(stockTransactions));
            var result = new List<TradingStockTransaction>();
            foreach (var stockTransaction in stockTransactions)
            {
                var stockPrice = stockPrices.FirstOrDefault(q => q.DatePath == stockTransaction.DatePath);
                var transactionDetails = stockTransaction.ZipDetails is not null ? JsonSerializer.Deserialize<List<StockTransactionDetails>>(zipHelper.UnZipByte(stockTransaction.ZipDetails)) : new();
                result.Add(new()
                {
                    DatePath = stockTransaction.DatePath,
                    Details = stockPrice is not null ? transactionDetails.RebuildStockTransactionDetails(stockPrice) : transactionDetails
                });
            }
            return result;
        }
    }
}