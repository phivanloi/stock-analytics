using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Services
{
    public static class StockTechnicalAnalytics
    {
        /// <summary>
        /// Phân tích quá mua, quá bán s rsi 14
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="quotes">Danh sách lịch sử giá sắp xếp ngày giao dịch tăng dần</param>
        /// <returns>double, int</returns>
        public static (double? StochRsi, int Score) StochRsiAnalytics(List<AnalyticsNote> notes, List<Quote> quotes)
        {
            if (quotes is null || quotes.Count <= 17)
            {
                return (null, notes.Add($"Chưa đủ dữ liệu đê phân tích stoch rsi 14.", -1, -1, null));
            }

            var type = 0;
            var score = 0;
            var note = string.Empty;

            try
            {
                var rsiResults = quotes.GetStochRsi(14, 14, 3, 3);
                if (rsiResults is null || rsiResults.Count() < 0)
                {
                    return (null, notes.Add($"Chưa đủ dữ liệu đê phân tích stoch rsi 14.", -1, -1, null));
                }

                var topThree = rsiResults.OrderByDescending(q => q.Date).Take(3).ToList();
                if (topThree[0].StochRsi > topThree[0].Signal)
                {
                    score++;
                    type++;
                    note += "Stoch Rsi 14 đang trên đường tiếng hiệu.";
                }
                else
                {
                    score--;
                    type--;
                    note += "Stoch Rsi 14 đang dưới đường tiếng hiệu.";
                }

                if (topThree[0].StochRsi.HasValue && topThree[1].StochRsi.HasValue && topThree[2].StochRsi.HasValue)
                {
                    if (topThree[0].StochRsi > 20 && topThree[1].StochRsi < 20 && topThree[2].StochRsi < topThree[1].StochRsi)
                    {
                        score++;
                        type++;
                        note += " Đang đi lên từ quá bán mạnh.";
                    }
                    if (topThree[0].StochRsi < 80 && topThree[1].StochRsi > 80 && topThree[2].StochRsi > topThree[1].StochRsi)
                    {
                        score--;
                        type--;
                        note += " Đang đi xuống từ quá mua mạnh.";
                    }
                }

                return (topThree[0].StochRsi, notes.Add(note, score, type, null));
            }
            catch//Đang có một lỗi khi lấy chỉ báo rsi lên tạm thời try lại
            {
                return (null, notes.Add($"Số liệu stoch rsi bị lỗi.", -1, -1, null));
            }
        }

        /// <summary>
        /// Phân tích dòng tiền của cổ phiếu
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="chartPrices">Lịch sửa giá của cổ phiếu</param>
        /// <returns>int</returns>
        public static int PriceTrend(List<AnalyticsNote> notes, List<ChartPrice> chartPrices)
        {
            if (chartPrices.Count < 2)
            {
                return notes.Add($"Chưa có đủ lịch sử giá để phân tích dòng tiên cho cổ phiếu.", -1000, -2, null);
            }

            var checkChartPrices = chartPrices.OrderByDescending(q => q.TradingDate).ToList();
            if (checkChartPrices[0].TotalMatchVol < 10000)
            {
                return notes.Add($"Cổ phiếu có thanh khoản thấp hơn 10000 cổ/1 phiên thì bỏ qua", -1000, -2, null);
            }
            var type = 0;
            var score = 0f;

            if (checkChartPrices.Count > 2)
            {
                score += checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[1].ClosePrice) * 5;
            }

            if (checkChartPrices.Count > 5)
            {
                score += checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[4].ClosePrice) * 3;
            }

            if (checkChartPrices.Count > 10)
            {
                score += checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[9].ClosePrice) * 2;
            }

            if (checkChartPrices.Count > 30)
            {
                score += checkChartPrices[0].ClosePrice.GetPercent(checkChartPrices[29].ClosePrice) * 1;
            }

            if (score > 10)
            {
                type = 1;
            }

            if (score > 50)
            {
                type = 2;
            }

            if (score < -10)
            {
                type = -1;
            }

            if (score < -50)
            {
                type = -2;
            }

            return notes.Add($"Biến động giá trong các khoảng thời gian 1,5,10,30 phiên {score:0,0.00} trọng số 5,3,2,1 tương ứng.", (int)score, type, null);
        }

        /// <summary>
        /// Phân tích su thế của giá cổ phiếu
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="chartPrices">Danh sách lịch sử giao dịch, sắp sếp theo phiên diao dịch gần nhất lên đầu</param>
        /// <param name="exchangeFluctuationsRate">Tỉ lệ tăng giảm tối đa của sàn mà chứng khoán liêm yết</param>
        /// <returns>int</returns>
        public static int LastTradingAnalytics(List<AnalyticsNote> notes, List<ChartPrice> chartPrices, float exchangeFluctuationsRate)
        {
            if (chartPrices.Count < 2)
            {
                return notes.Add($"Chưa có đủ lịch sử giá để phân tích biến động giá phiên cuối.", -5, -1, null);
            }

            var type = 0;
            var score = 0;
            var note = string.Empty;

            var priceChange = chartPrices[0].ClosePrice.GetPercent(chartPrices[1].ClosePrice);
            if (Math.Abs(priceChange) > (exchangeFluctuationsRate - (exchangeFluctuationsRate * 0.15f)))//Mức biến động giá lớn
            {
                if (priceChange < 0)
                {
                    note += $"Phiên cuối, giá có mức biển động giảm lớn({priceChange:0.0}%),";
                    type = -1;
                    score -= 1;
                }
                else
                {
                    note += $"Phiên cuối, giá có mức biển động tăng lớn({priceChange:0.0}%),";
                    score += 1;
                    type = 1;
                }
            }

            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Phân tích chỉ báo stochastic
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="indicators">Danh sách chỉ báo kỹ thuật</param>
        /// <returns></returns>
        public static int StochasticTrend(List<AnalyticsNote> notes, Dictionary<string, IndicatorSet> indicators)
        {
            if (indicators.Count < 2)
            {
                return notes.Add($"Chưa có đủ danh sách chỉ báo để phân tích phân tích Stochastic.", -1, -1, null);
            }

            var type = 0;
            var score = 0;
            string note;
            var checkList = indicators.Select(q => q.Value).ToList();
            if (checkList[0].Values["stochastic-14"] > 80)
            {
                score--;
                type--;
                note = $"Chỉ báo Stochastic(14) {checkList[0].Values["stochastic-14"]:0.0} đang ở trạng thái quá mua vượt quá 80.";
            }
            else if (checkList[0].Values["stochastic-14"] < 20)
            {
                score++;
                type++;
                note = $"Chỉ báo Stochastic(14) {checkList[0].Values["stochastic-14"]:0.0} đang ở trạng thái quá bán vượt quá 20.";
            }
            else
            {
                note = $"Chỉ báo Stochastic(14) {checkList[0].Values["stochastic-14"]:0.0} đang ở trạng thái bình thường.";
            }

            if (checkList[0].Values["stochastic-14"] > (checkList[1].Values["stochastic-14"] + (checkList[1].Values["stochastic-14"] * 0.05f)))
            {
                note += " Chỉ báo Stochastic(14) phiên gần nhất lớn hơn phiên trước + 5% đó.";
                score++;
            }
            else
            {
                note += " Chỉ báo Stochastic(14) phiên gần nhất nhỏ hơn phiên trước + 5% đó.";
                score--;
            }

            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Phân tích su thế tăng trưởng của thị giá theo đường ema
        /// </summary>
        /// <param name="notes">Danh sách ghi chú</param>
        /// <param name="indicators">Chỉ báo quỹ thuật của lịch sử giá cần phân tích</param>
        /// <param name="lastTradingPrice">Thông tin phiên giao dịch cuối cùng</param>
        /// <returns></returns>
        public static int EmaTrend(List<AnalyticsNote> notes, Dictionary<string, IndicatorSet> indicators, StockPrice lastTradingPrice)
        {
            if (indicators.Count < 1)
            {
                return notes.Add($"Chưa có đủ danh sách chỉ báo để phân tích phân tích.", -1, -1, null);
            }

            var type = 0;
            var score = 0;
            var note = string.Empty;
            var lastIndicator = indicators.FirstOrDefault().Value;

            if (lastIndicator is not null)
            {
                if (lastIndicator.Values["ema-3"] < lastTradingPrice.ClosePrice)
                {
                    score += 2;
                }
                else
                {
                    score -= 2;
                }
                if (lastIndicator.Values["ema-5"] < lastTradingPrice.ClosePrice)
                {
                    score++;
                }
                else
                {
                    score--;
                }
                if (lastIndicator.Values["ema-10"] < lastTradingPrice.ClosePrice)
                {
                    score++;
                }
                else
                {
                    score--;
                }
                if (lastIndicator.Values["ema-20"] < lastTradingPrice.ClosePrice)
                {
                    score++;
                }
                else
                {
                    score--;
                }
                if (lastIndicator.Values["ema-50"] < lastTradingPrice.ClosePrice)
                {
                    score++;
                }
                else
                {
                    score--;
                }
                if (lastIndicator.Values["ema-100"] < lastTradingPrice.ClosePrice)
                {
                    score++;
                }
                else
                {
                    score--;
                }
                if (lastIndicator.Values["ema-200"] < lastTradingPrice.ClosePrice)
                {
                    score++;
                }
                else
                {
                    score--;
                }
            }

            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Kiểm tra khối lượng giao dịch trung bình 5 phiên
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Danh sách lịch sử giá</param>
        /// <returns>int</returns>
        public static int MatchVolCheck(List<AnalyticsNote> notes, List<StockPrice> stockPrices)
        {
            var avgMatchVol = stockPrices.Average(q => q.TotalMatchVol);
            var count = stockPrices.Count;
            if (avgMatchVol < 100000)
            {
                return notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} nhỏ hơn 100.000", -2, -2, null);
            }
            else if (avgMatchVol <= 500000)
            {
                return notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} nhỏ hơn hoặc bằng 500.000", 0, 0, null);
            }
            else if (avgMatchVol <= 1500000)
            {
                return notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} nhỏ hơn hoặc bằng 1.500.000", 0, 0, null);
            }
            else
            {
                return notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} lớn hơn 1.500.000", 2, 2, null);
            }
        }

        /// <summary>
        /// Phân tích khối lượng khớp lệnh
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Lịch sửa giá của cổ phiếu</param>
        /// <returns>int</returns>
        public static int MatchVolTrend(List<AnalyticsNote> notes, List<StockPrice> stockPrices)
        {
            if (stockPrices.Count < 1)
            {
                return notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
            }

            var type = 0;
            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = stockPrices.GetFluctuationsTopDown(q => q.TotalMatchVol);
            var avgPercent = percents.Average();
            var note = $"Tăng trưởng khối lượng khớp lệnh trung bình {stockPrices.Count} phiên {avgPercent:0,0}%";
            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} phiên đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} phiên đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} phiên đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} phiên. ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} phiên. ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} phiên. ";
            }

            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Phân tích lực mua của khối ngoại
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Lịch sửa giá của cổ phiếu</param>
        /// <returns>int</returns>
        public static int ForeignPurchasingPowerTrend(List<AnalyticsNote> notes, List<StockPrice> stockPrices)
        {
            if (stockPrices.Count < 1)
            {
                return notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
            }

            var type = 0;
            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = stockPrices.GetFluctuationsTopDown(q => q.ForeignBuyVolTotal - q.ForeignSellVolTotal);

            var avgPercent = percents.Average();
            var note = $"Tăng trưởng mua dòng khối ngoại trung bình {stockPrices.Count} phiên {avgPercent:0,0}%";
            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} phiên đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} phiên đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} phiên đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} phiên, ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} phiên, ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} phiên, ";
            }
            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Phân tích só người tham gia mua, bán
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Lịch sửa giá của cổ phiếu</param>
        /// <returns>int</returns>
        public static int TraderTrend(List<AnalyticsNote> notes, List<StockPrice> stockPrices)
        {
            if (stockPrices.Count < 1)
            {
                return notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
            }

            var type = 0;
            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = stockPrices.GetFluctuationsTopDown(q => q.TotalBuyTrade - q.TotalSellTrade);
            var avgPercent = percents.Average();
            var note = $"Tăng trưởng người tham gia giao dịch trung bình {stockPrices.Count} phiên {avgPercent:0,0}%";
            if (fistEqualCount > 0)
            {
                note += $", duy trì trong {fistEqualCount} phiên đến hiện tại.";
            }
            if (fistGrowCount > 0)
            {
                score += fistGrowCount;
                note += $", tăng trong {fistGrowCount} phiên đến hiện tại.";
                type++;
            }
            if (fistDropCount > 0)
            {
                score -= fistDropCount;
                note += $", giảm trong {fistDropCount} phiên đến hiện tại.";
                type--;
            }

            note += $" Tổng biến động:";
            if (equalCount > 0)
            {
                note += $" duy trì {equalCount} phiên. ";
            }
            if (increaseCount > 0)
            {
                note += $" tăng {increaseCount} phiên. ";
            }
            if (reductionCount > 0)
            {
                note += $" giảm {reductionCount} phiên. ";
            }

            var totalBuyer = stockPrices.Sum(q => q.TotalBuyTrade);
            var totalSeller = stockPrices.Sum(q => q.TotalSellTrade);

            if (totalBuyer > totalSeller)
            {
                note += $" Trong 10 phiên tổng số người mua {totalBuyer:0,0} lớn hơn số người bán {totalSeller:0,0} ({totalBuyer.GetPercent(totalSeller):0.00}%)";
                type++;
                score++;
            }
            else
            {
                note += $" Trong 10 phiên tổng số người bán {totalSeller:0,0} lớn hơn số người mua {totalBuyer:0,0} ({totalSeller.GetPercent(totalBuyer):0.00}%)";
                type--;
                score--;
            }

            return notes.Add(note, score, type, null);
        }

        /// <summary>
        /// Đánh gía giao dịch của vnd
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="fiinEvaluate">Đánh giá của fiin</param>
        /// <returns></returns>
        public static int FiinCheck(List<AnalyticsNote> notes, FiinEvaluated? fiinEvaluate)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (fiinEvaluate is null)
            {
                note += "Không có thông tin đánh giá củ fiintrading. ";
                score -= 2;
                type--;

                return notes.Add(note, score, type, null);
            }

            if (fiinEvaluate.ControlStatusCode >= 0)
            {
                note += $"Trạng thái giao dịch cổ phiếu {fiinEvaluate.ControlStatusName}. ";
                score -= 10;
                type -= 2;
            }

            note += $"Fiin đánh giá động lực của thị giá điểm {fiinEvaluate.Momentum}, ";
            switch (fiinEvaluate.Momentum)
            {
                case "A":
                    score += 2;
                    type = 2;
                    break;
                case "B":
                    score++;
                    type = 1;
                    break;
                case "C":
                    break;
                case "D":
                    score--;
                    type = -1;
                    break;
                case "E":
                    score -= 1;
                    type = -2;
                    break;
                case "F":
                    score -= 2;
                    type = -2;
                    break;
            }

            return notes.Add(note, score, type, null);
        }
    }
}