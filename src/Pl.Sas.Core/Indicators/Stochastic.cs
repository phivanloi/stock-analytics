using Ardalis.GuardClauses;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Indicators
{
    public static class Stochastic
    {
        /// <summary>
        /// Tính toán chỉ báo stochastic, khoản thời gian tính toán bằng số lịch sử giá truyền vào
        /// </summary>
        /// <param name="stockPrices">Danh sách lịch sử giá cần tính xắp xếp theo ngày giao dịch giảm dần</param>
        /// <returns></returns>
        public static float CalculateStochastic(List<StockPrice> stockPrices)
        {
            Guard.Against.NullOrEmpty(stockPrices, nameof(stockPrices));
            var minPrice = stockPrices.Min(q => q.LowestPrice);
            var maxPrice = stockPrices.Max(q => q.HighestPrice);
            var currentPrice = stockPrices[0].ClosePrice;
            if (Math.Round(maxPrice, 2) - Math.Round(minPrice, 2) == 0)
            {
                return 0;
            }
            return (currentPrice - minPrice) / (maxPrice - minPrice) * 100;
        }

        /// <summary>
        /// Tính toán một danh sách chỉ số stochastic
        /// </summary>
        /// <param name="stockPrices">Danh sách cần tính toán xắp xếp theo ngày giao dịch tăng dần</param>
        /// <returns>Đầu ra là danh sách chỉ số được sắp sếp theo tứ tự ngày giao dịch tăng dần</returns>
        public static List<float> CalculateStochastics(List<StockPrice> stockPrices)
        {
            var results = new List<float>();
            if (stockPrices.Count > 14)
            {
                for (int i = 0; i < stockPrices.Count; i++)
                {
                    if (i < 14)
                    {
                        continue;
                    }

                    var histories = stockPrices.Skip(i - 13).Take(14).OrderByDescending(q => q.TradingDate).ToList();
                    results.Add(CalculateStochastic(histories));
                }
            }
            return results;
        }
    }
}