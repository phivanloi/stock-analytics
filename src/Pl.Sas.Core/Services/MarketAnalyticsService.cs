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
        public static int IndexValueTrend(List<AnalyticsNote> notes, List<ChartPrice> chartPrices, Dictionary<string, IndicatorSet> indicatorSets)
        {
            if (chartPrices.Count < 2)
            {
                notes.Add($"Chưa có đủ lịch sử chỉ số để phân tích su thế thị trường.", -1, -1, null);
                return -1;
            }

            var type = 0;
            var score = 0;
            var (fistEqualCount, fistGrowCount, fistDropCount) = chartPrices.GetTrendCountTopDown(q => q.ClosePrice);

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
            var changePecent = chartPrices[0].ClosePrice.GetPercent(chartPrices[1].ClosePrice);
            if (changePecent > 0)
            {
                if (changePecent > 1.5f)
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
                if (changePecent < -1.5f)
                {
                    subScore -= 2;
                }
                else
                {
                    subScore--;
                }
            }

            if (indicatorSets.Count > 1)
            {
                if (chartPrices[^1].ClosePrice > indicatorSets[chartPrices[^1].DatePath].Values[$"ema-5"])
                {
                    note += "Chỉ số trên đường ema 5. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 5. ";
                    subScore--;
                }
                if (chartPrices[^1].ClosePrice > indicatorSets[chartPrices[^1].DatePath].Values[$"ema-10"])
                {
                    note += "Chỉ số trên đường ema 10. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 10. ";
                    subScore--;
                }
                if (chartPrices[^1].ClosePrice > indicatorSets[chartPrices[^1].DatePath].Values[$"ema-20"])
                {
                    note += "Chỉ số trên đường ema 20. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 20. ";
                    subScore--;
                }
                if (chartPrices[^1].ClosePrice > indicatorSets[chartPrices[^1].DatePath].Values[$"ema-50"])
                {
                    note += "Chỉ số trên đường ema 50. ";
                    subScore++;
                }
                else
                {
                    note += "Chỉ số dưới đường ema 50. ";
                    subScore--;
                }
                if (chartPrices[^1].ClosePrice > indicatorSets[chartPrices[^1].DatePath].Values[$"ema-100"])
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
        /// Phân tích chỉ số tâm lý thị trường
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="marketSentimentScore">Điểm đánh giá thị trường</param>
        /// <returns>int</returns>
        public static int MarketSentimentTrend(List<AnalyticsNote> notes, float marketSentimentScore)
        {
            var type = 0;
            var score = 0;
            var note = $"Chỉ số tâm lý thị trường hiện tại {marketSentimentScore}";
            notes.Add(note, score, type, null);
            return score;
        }

        /// <summary>
        /// Phân tích trạng thái ngành
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="industryAnalytics">Thông tin ngành của cố phiếu đang lắm giữ</param>
        /// <returns></returns>
        public static int IndustryTrend(List<AnalyticsNote> notes, IndustryAnalytics? industryAnalytics)
        {
            if (industryAnalytics is null)
            {
                notes.Add($"Không có thông tin ngành để phân tích.", -2, -2, null);
                return -2;
            }

            var type = 0;
            var score = 0;
            var note = $"Cổ phiếu thuộc ngành \"{industryAnalytics.Code}\" có đánh giá cố định {industryAnalytics.ManualScore}, đánh giá tự động {industryAnalytics.Score}.";
            if (industryAnalytics.Score > 100)
            {
                type++;
                score++;
            }
            else if (industryAnalytics.Score < 0)
            {
                type--;
                score--;
            }
            if (industryAnalytics.ManualScore > 0)
            {
                type++;
                score++;
            }
            else
            {
                type--;
                score--;
            }
            notes.Add(note, score, type, null);
            return score;
        }
    }
}