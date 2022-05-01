using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Services
{
    public static class StockAnalyticsService
    {
        /// <summary>
        /// Phân tích su thế của giá cổ phiếu
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Danh sách lịch sử giao dịch, sắp sếp theo phiên diao dịch gần nhất lên đầu</param>
        /// <param name="exchangeFluctuationsRate">Tỉ lệ tăng giảm tối đa của sàn mà chứng khoán liêm yết</param>
        /// <returns>int</returns>
        public static int LastTradingAnalytics(List<AnalyticsNote> notes, List<StockPrice> stockPrices, float exchangeFluctuationsRate)
        {
            if (stockPrices.Count < 2)
            {
                notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -5, -1, null);
                return -5;
            }

            var type = 0;
            var score = 0;
            var note = string.Empty;

            var priceChange = stockPrices[0].ClosePrice.GetPercent(stockPrices[1].ClosePrice);
            if (priceChange == 0)// Giá không có biến động
            {
                note += $"Giá không thay đổi,";
            }
            else if (Math.Abs(priceChange) > (exchangeFluctuationsRate - (exchangeFluctuationsRate * 0.15f)))//Mức biến động giá lớn
            {
                if (priceChange < 0)
                {
                    note += $"Phiên cuối, giá có mức biển động giảm lớn({priceChange:0.0}%),";
                    type = -3;
                    score -= 3;
                }
                else
                {
                    note += $"Phiên cuối, giá có mức biển động tăng lớn({priceChange:0.0}%),";
                    score += 3;
                    type = 3;
                }
            }
            else if (Math.Abs(priceChange) > (exchangeFluctuationsRate - (exchangeFluctuationsRate * 0.5f)))//Mức biến động giá khá lớn
            {
                if (priceChange < 0)
                {
                    note += $"Giá có mức biển động giảm khá lớn({priceChange:0.0}%) trong phiên cuối cùng,";
                    type = -2;
                    score -= 2;
                }
                else
                {
                    note += $"Giá có mức biển động tăng khá lớn({priceChange:0.0}%) trong phiên cuối cùng,";
                    score += 2;
                    type = 2;
                }
            }
            else
            {
                if (priceChange < 0)
                {
                    note += $"Giá có mức biển động giảm bình thường({priceChange:0.0}%) trong phiên cuối cùng,";
                    score -= 1;
                    type--;
                }
                else
                {
                    note += $"Giá có mức biển động tăng bình thường({priceChange:0.0}%) trong phiên cuối cùng,";
                    score += 1;
                    type++;
                }
            }


            notes.Add(note, score, type, null);
            return score;
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
                notes.Add($"Chưa có đủ danh sách chỉ báo để phân tích phân tích Stochastic.", -1, -1, null);
                return -1;
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

            notes.Add(note, score, type, null);
            return score;
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
                notes.Add($"Chưa có đủ danh sách chỉ báo để phân tích phân tích.", -1, -1, null);
                return -1;
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

            notes.Add(note, score, type, null);
            return score;
        }

        /// <summary>
        /// Phân tích su thế của giá cổ phiếu
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="stockPrices">Lịch sửa giá của cổ phiếu</param>
        /// <returns>int</returns>
        public static int PriceTrend(List<AnalyticsNote> notes, List<StockPrice> stockPrices)
        {
            if (stockPrices.Count < 1)
            {
                notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
                return -1;
            }

            var type = 0;
            var score = 0;
            var (equalCount, increaseCount, reductionCount, fistEqualCount, fistGrowCount, fistDropCount, percents) = stockPrices.GetFluctuationsTopDown(q => q.ClosePrice);

            var avgPercent = percents.Average();
            var note = $"Tăng trưởng giá trung bình {stockPrices.Count} phiên {avgPercent:0,0}%";
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

            if (stockPrices.Count > 5)
            {
                if (stockPrices[0].ClosePrice > stockPrices[5].ClosePrice)
                {
                    note += $"Biến động giá 5 phiên tăng {stockPrices[0].ClosePrice.GetPercent(stockPrices[5].ClosePrice):0,0}%.";
                }
                else
                {
                    note += $"Biến động giá 5 phiên giảm {stockPrices[0].ClosePrice.GetPercent(stockPrices[5].ClosePrice):0,0}%.";
                }
            }

            if (stockPrices.Count > 10)
            {
                if (stockPrices[0].ClosePrice > stockPrices[10].ClosePrice)
                {
                    note += $"Biến động giá 10 phiên tăng {stockPrices[0].ClosePrice.GetPercent(stockPrices[10].ClosePrice):0,0}%.";
                }
                else
                {
                    note += $"Biến động giá 10 phiên giảm {stockPrices[0].ClosePrice.GetPercent(stockPrices[10].ClosePrice):0,0}%.";
                }
            }

            if (stockPrices.Count > 20)
            {
                if (stockPrices[0].ClosePrice > stockPrices[20].ClosePrice)
                {
                    note += $"Biến động giá 20 phiên tăng {stockPrices[0].ClosePrice.GetPercent(stockPrices[20].ClosePrice):0,0}%.";
                }
                else
                {
                    note += $"Biến động giá 20 phiên giảm {stockPrices[0].ClosePrice.GetPercent(stockPrices[20].ClosePrice):0,0}%.";
                }
            }

            if (stockPrices.Count > 50)
            {
                if (stockPrices[0].ClosePrice > stockPrices[50].ClosePrice)
                {
                    note += $"Biến động giá 50 phiên tăng {stockPrices[0].ClosePrice.GetPercent(stockPrices[50].ClosePrice):0,0}%.";
                }
                else
                {
                    note += $"Biến động giá 50 phiên giảm {stockPrices[0].ClosePrice.GetPercent(stockPrices[50].ClosePrice):0,0}%.";
                }
            }

            notes.Add(note, score, type, null);
            return score;
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
                notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} nhỏ hơn 100.000", -2, -2, null);
                return -2;
            }
            else if (avgMatchVol <= 500000)
            {
                notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} nhỏ hơn hoặc bằng 500.000", 0, 0, null);
                return 0;
            }
            else if (avgMatchVol <= 1500000)
            {
                notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} nhỏ hơn hoặc bằng 1.500.000", 0, 0, null);
                return 1;
            }
            else
            {
                notes.Add($"Khối lượng giao dịch trung bình {count} phiên là {avgMatchVol:0,0} lớn hơn 1.500.000", 2, 2, null);
                return 2;
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
                notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
                return -1;
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

            notes.Add(note, score, type, null);
            return score;
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
                notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
                return -1;
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
            notes.Add(note, score, type, null);
            return score;
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
                notes.Add($"Chưa có đủ lịch sử giá để phân tích.", -1, -1, null);
                return -1;
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

            notes.Add(note, score, type, null);
            return score;
        }

        /// <summary>
        /// Đánh gía giao dịch của vnd
        /// </summary>
        /// <param name="notes">Ghi chú</param>
        /// <param name="fiinEvaluate">Đánh giá của fiin</param>
        /// <returns></returns>
        public static int FiinCheck(List<AnalyticsNote> notes, FiinEvaluated fiinEvaluate)
        {
            var type = 0;
            var score = 0;
            var note = string.Empty;

            if (fiinEvaluate is null)
            {
                note += "Không có thông tin đánh giá củ fiintrading. ";
                score -= 2;
                type--;

                notes.Add(note, score, type, null);
                return score;
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

            notes.Add(note, score, type, null);
            return score;
        }
    }
}