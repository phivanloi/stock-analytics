using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Services
{
    public static class MarketAnalyticsService
    {
        /// <summary>
        /// Phân tích só người tham gia mua, bán
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Lịch sửa giá của cổ phiếu</param>
        /// <param name="indicatorSets">Tập hợp các chỉ báo kỹ thuật của index</param>
        /// <returns>int</returns>
        public static int IndexValueTrend(List<AnalyticsMessage> notes, List<StockPrice> stockPrices, Dictionary<string, IndicatorSet> indicatorSets)
        {
            if (stockPrices?.Count < 2)
            {
                notes.Add($"Chưa có đủ lịch sử chỉ số để phân tích su thế thị trường.", -1, -1, null);
                return -1;
            }

            var type = 0;
            var score = 0;
            var (fistEqualCount, fistGrowCount, fistDropCount) = stockPrices.GetTrendCountTopDown(q => q.ClosePrice);

            var note = $"";
            if (fistEqualCount > 0)
            {
                note += $"Chỉ số duy trì trong {fistEqualCount} phiên đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $"Chỉ số tăng trong {fistGrowCount} phiên đến hiện tại.";
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $"Chỉ số giảm trong {fistDropCount} phiên đến hiện tại.";
            }

            var subScore = 0;
            var changePecent = stockPrices[0].ClosePrice.GetPercent(stockPrices[1].ClosePrice);
            if (changePecent > 0)
            {
                if (changePecent > 1.5M)
                {
                    subScore += 2;
                }
                else
                {
                    subScore++;
                }
            }
            else
            {
                if (changePecent < -1.5M)
                {
                    subScore -= 2;
                }
                else
                {
                    subScore--;
                }
            }

            if (indicatorSets?.Count > 1)
            {
                if (stockPrices[0].ClosePriceAdjusted > indicatorSets[stockPrices[0].DatePath].Values[$"ema-5"])
                {
                    note += "Chỉ số trên đường ema 5. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 5. ";
                    subScore--;
                }
                if (stockPrices[0].ClosePriceAdjusted > indicatorSets[stockPrices[0].DatePath].Values[$"ema-10"])
                {
                    note += "Chỉ số trên đường ema 10. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 10. ";
                    subScore--;
                }
                if (stockPrices[0].ClosePriceAdjusted > indicatorSets[stockPrices[0].DatePath].Values[$"ema-20"])
                {
                    note += "Chỉ số trên đường ema 20. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 20. ";
                    subScore--;
                }
                if (stockPrices[0].ClosePriceAdjusted > indicatorSets[stockPrices[0].DatePath].Values[$"ema-50"])
                {
                    note += "Chỉ số trên đường ema 50. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 50. ";
                    subScore--;
                }
                if (stockPrices[0].ClosePriceAdjusted > indicatorSets[stockPrices[0].DatePath].Values[$"ema-100"])
                {
                    note += "Chỉ số trên đường ema 100. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 100. ";
                    subScore--;
                }
            }

            score += subScore;
            if (score > 0)
            {
                type++;
            }
            else if (score < 0)
            {
                type--;
            }
            notes.Add(note, score, type, "https://vcbs.com.vn/vn/Utilities/Index/52");
            return score;
        }

        /// <summary>
        /// Kiểm tra khối lượng giao dịch trung bình 5 phiên, hiện chưa xử lý
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Danh sách lịch sử giá</param>
        /// <returns>int</returns>
        public static int MatchVolTrend(List<AnalyticsMessage> notes, List<StockPrice> stockPrices)
        {
            if (stockPrices?.Count < 2)
            {
                notes.Add($"Chưa có đủ lịch sử chỉ số để phân tích khối lượng thị trường.", -1, -1, null);
                return -1;
            }

            //var type = 0;
            var score = 0;
            //var note = "";
            var priceArray = stockPrices.ToArray();
            var avgMatchVol = stockPrices.ToArray()[..2].Average(q => q.TotalMatchVol);
            var avgMatchVolLast = stockPrices.ToArray()[2..5].Average(q => q.TotalMatchVol);
            var changePecent = avgMatchVol.GetPercent(avgMatchVolLast);
            if (changePecent > 0)
            {
                score++;
            }
            else
            {
                score--;
            }
            //notes.Add(note, score, type, null);
            return 0;
        }
    }
}