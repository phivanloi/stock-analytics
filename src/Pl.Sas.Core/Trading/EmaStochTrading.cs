using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Trading
{
    public class EmaStochTrading : BaseTrading
    {
        /// <summary>
        /// Hàm đánh giá xem hôm nay có lên mua vào cổ phiếu hay không
        /// </summary>
        /// <param name="tradingCase">Case trường hợp đầu tư</param>
        /// <param name="indicatorSet">Tập hợp các chỉ báo của phiên trước đó</param>
        /// <returns>bool</returns>
        public static bool TodayIsBuy(TradingCase tradingCase, IndicatorSet indicatorSet)
        {
            if (indicatorSet is null)
            {
                return false;
            }
            if (indicatorSet.Values[$"stochastic-{tradingCase.Stochastic}"] > tradingCase.LowestStochasticBuy)
            {
                return false;
            }
            return indicatorSet.Values[$"ema-{tradingCase.FirstEmaBuy}"] > indicatorSet.Values[$"ema-{tradingCase.SecondEmaBuy}"];
        }

        /// <summary>
        /// Hàm đánh giá xem hôm nay có lên mua vào cổ phiếu hay không
        /// </summary>
        /// <param name="tradingCase">Case trường hợp đầu tư</param>
        /// <param name="indicatorSet">Tập hợp các chỉ báo của phiên trước đó</param>
        /// <returns>bool</returns>
        public static bool TodayIsSell(TradingCase tradingCase, IndicatorSet indicatorSet)
        {
            if (indicatorSet is null)
            {
                return false;
            }
            if (indicatorSet.Values[$"stochastic-{tradingCase.Stochastic}"] < tradingCase.HighestStochasticSell)
            {
                return false;
            }
            return indicatorSet.Values[$"ema-{tradingCase.FirstEmaSell}"] < indicatorSet.Values[$"ema-{tradingCase.SecondEmaSell}"];
        }

        /// <summary>
        /// Hàm tính toán giá tối ưu để mua
        /// </summary>
        /// <param name="stockPriceAdj">Lịch sử phiên giao dịch trước đó</param>
        /// <returns>decimal</returns>
        public static decimal CalculateOptimalBuyPrice(StockPriceAdj stockPriceAdj)
        {
            var buyPrice = stockPriceAdj.ClosePrice - (stockPriceAdj.ClosePrice / 100 * Math.Abs(stockPriceAdj.LowestPrice.GetPercent(stockPriceAdj.OpenPrice)));
            return Math.Round(buyPrice, 2);
        }

        /// <summary>
        /// Hàm tính toán giá tối ưu để bán
        /// </summary>
        /// <param name="stockPriceAdj">Lịch sử phiên giao dịch trước đó</param>
        /// <returns>decimal</returns>
        public static decimal CalculateOptimalSellPriceOnLoss(StockPriceAdj stockPriceAdj)
        {
            var sellPrice = stockPriceAdj.ClosePrice + (stockPriceAdj.ClosePrice / 100 * Math.Abs(stockPriceAdj.OpenPrice.GetPercent(stockPriceAdj.HighestPrice)));
            return Math.Round(sellPrice, 2);
        }

        /// <summary>
        /// Hàm giả lập quá trình mua bán cổ phiếu cho phương pháp đầu tu ngắn hạn
        /// </summary>
        /// <param name="tradingCase">Thông tin kiểm nghiệp giao dịch</param>
        /// <param name="stockPriceAdjs">Danh sách lịch sử cổ phiểu cần giả lập mua bán. Chú ý đã được xắp xếp tăng dần ngày giao dịch OrderBy(q => q.TradingDate)</param>
        /// <param name="indicatorSet">Tập hợp các chỉ báo</param>
        /// <param name="tradingStockTransactions">Danh sách lịch sử khớp lệnh</param>
        /// <param name="isNoteTrading">Cho phép ghi lịch sử giao dịch</param>
        /// <returns>decimal profit</returns>
        public static (bool IsBuy, bool IsSell) Trading(TradingCase tradingCase, List<StockPriceAdj> stockPriceAdjs, Dictionary<string, IndicatorSet> indicatorSet, bool isNoteTrading = false)
        {
            var capital = tradingCase.FixedCapital;
            var numberStock = 0M;
            var numberDayBuy = 0;
            var tradingHistory = new List<StockPriceAdj>();
            decimal? lastBuyPrice = null;
            StockPriceAdj previousDay = null;

            foreach (var day in stockPriceAdjs)
            {
                var hasIndicator = indicatorSet.TryGetValue(previousDay?.DatePath ?? "", out IndicatorSet indicator);
                if (!hasIndicator)
                {
                    if (isNoteTrading)
                    {
                        tradingCase.ExplainNotes.Add(new(0, $"Ngày {day.TradingDate:dd/MM/yyyy}: O: {day.OpenPrice / 1000:0,0.00}, H:{day.HighestPrice / 1000:0,0.00}, L:{day.LowestPrice / 1000:0,0.00}, C: {day.ClosePrice / 1000:0,0.00} => Không giao dịch do không có chỉ báo, tổng tài sản {capital:0,0}"));
                    }
                    previousDay = day;
                    continue;
                }
                var subEma = indicator.Values[$"ema-{tradingCase.FirstEmaSell}"] - indicator.Values[$"ema-{tradingCase.SecondEmaSell}"];

                if (lastBuyPrice is null)
                {
                    var isBuy = TodayIsBuy(tradingCase, indicator);
                    if (isBuy)
                    {
                        var optimalBuyPrice = CalculateOptimalBuyPrice(previousDay);
                        var buyPrice = optimalBuyPrice;
                        if (buyPrice <= day.LowestPrice)
                        {
                            buyPrice = day.ClosePrice;
                        }
                        var (stockCount, excessCash, totalTax) = Buy(capital, BuyTax, buyPrice);
                        capital = excessCash;
                        tradingCase.TotalTax += totalTax;
                        numberStock = stockCount;
                        lastBuyPrice = buyPrice;
                        numberDayBuy = 0;
                        if (isNoteTrading)
                        {
                            var showCapital = capital + (numberStock * day.ClosePrice);
                            tradingCase.ExplainNotes.Add(new(0, $"Ngày {day.TradingDate:dd/MM/yyyy}: O: {day.OpenPrice / 1000:0,0.00}, H:{day.HighestPrice / 1000:0,0.00}, L:{day.LowestPrice / 1000:0,0.00}, C: {day.ClosePrice / 1000:0,0.00}, ema {tradingCase.FirstEmaSell}-{tradingCase.SecondEmaSell}: {subEma:0.0} => Mua {numberStock:0,0} cổ phiếu với giá {buyPrice / 1000:0,0.00} ({optimalBuyPrice / 1000:0,0.00}), thuế {totalTax:0,0}. Tổng tài sản {showCapital:0,0}"));
                        }
                    }
                    else
                    {
                        if (isNoteTrading)
                        {
                            tradingCase.ExplainNotes.Add(new(0, $"Ngày {day.TradingDate:dd/MM/yyyy}: O: {day.OpenPrice / 1000:0,0.00}, H:{day.HighestPrice / 1000:0,0.00}, L:{day.LowestPrice / 1000:0,0.00}, C: {day.ClosePrice / 1000:0,0.00}, ema {tradingCase.FirstEmaSell}-{tradingCase.SecondEmaSell}: {subEma:0.0} => Không giao dịch. Tổng tài sản {capital:0,0}"));
                        }
                    }
                }
                else
                {
                    numberDayBuy++;
                    if (numberDayBuy > 2)
                    {
                        var isSell = TodayIsSell(tradingCase, indicator);
                        if (isSell)
                        {
                            var optimalSellPrice = CalculateOptimalSellPriceOnLoss(previousDay);
                            var sellPrice = optimalSellPrice;
                            if (optimalSellPrice >= day.HighestPrice)
                            {
                                sellPrice = day.ClosePrice;
                            }
                            var (totalProfit, totalTax) = Sell(numberStock, SellTax, sellPrice);
                            capital = totalProfit;
                            tradingCase.TotalTax += totalTax;
                            if (isNoteTrading)
                            {
                                tradingCase.ExplainNotes.Add(new(lastBuyPrice < sellPrice ? 1 : -1, $"Ngày {day.TradingDate:dd/MM/yyyy}: O: {day.OpenPrice / 1000:0,0.00}, H:{day.HighestPrice / 1000:0,0.00}, L:{day.LowestPrice / 1000:0,0.00}, C: {day.ClosePrice / 1000:0,0.00}, ema {tradingCase.FirstEmaSell}-{tradingCase.SecondEmaSell}: {subEma:0.0}, bán {numberStock:0,0} cổ phiếu với giá {sellPrice / 1000:0,0.00} ({optimalSellPrice / 1000:0,0.00}), thuế {totalTax:0,0}. Tổng tài sản {capital:0,0}."));
                            }
                            lastBuyPrice = null;
                        }
                        else
                        {
                            if (isNoteTrading)
                            {
                                var showCapital = capital + (numberStock * day.ClosePrice);
                                tradingCase.ExplainNotes.Add(new(0, $"Ngày {day.TradingDate:dd/MM/yyyy}: O: {day.OpenPrice / 1000:0,0.00}, H:{day.HighestPrice / 1000:0,0.00}, L:{day.LowestPrice / 1000:0,0.00}, C: {day.ClosePrice / 1000:0,0.00}, ema {tradingCase.FirstEmaSell}-{tradingCase.SecondEmaSell}: {subEma:0.0} => Không giao dịch, số cổ phiếu {numberStock}. Tổng tài sản {showCapital:0,0}."));
                            }
                        }
                    }
                    else
                    {
                        if (isNoteTrading)
                        {
                            var showCapital = capital + (numberStock * day.ClosePrice);
                            tradingCase.ExplainNotes.Add(new(0, $"Ngày {day.TradingDate:dd/MM/yyyy}: O: {day.OpenPrice / 1000:0,0.00}, H:{day.HighestPrice / 1000:0,0.00}, L:{day.LowestPrice / 1000:0,0.00}, C: {day.ClosePrice / 1000:0,0.00}, ema {tradingCase.FirstEmaSell}-{tradingCase.SecondEmaSell}: {subEma:0.0} => Không giao dịch, số cổ phiếu {numberStock}. Tổng tài sản {showCapital:0,0}."));
                        }
                    }
                }

                previousDay = day;
                tradingHistory.Add(day);
            }
            var lastHistory = stockPriceAdjs[^1];
            var lastIndicatorSet = indicatorSet[previousDay.DatePath];
            var todayIsBuy = false;
            var todayIsSell = false;
            if (lastBuyPrice != null)
            {
                tradingCase.Profit = capital + (numberStock * lastHistory.ClosePrice);
                todayIsSell = numberDayBuy > 2 && TodayIsSell(tradingCase, lastIndicatorSet);
            }
            else
            {
                tradingCase.Profit = capital;
                todayIsBuy = TodayIsBuy(tradingCase, lastIndicatorSet);
            }

            return (todayIsBuy, todayIsSell);
        }

        /// <summary>
        /// Hàm tạo ra các trường hợp trading
        /// </summary>
        /// <param name="exchangeFluctuationsRate">Tỉ lệ biến động trong một phiên của sàn giao dịch</param>
        /// <param name="fixedCapital">Vốn ban đầu</param>
        /// <returns>HashSet TradingCase</returns>
        public static TradingCase[] BuildTradingCases(decimal fixedCapital = 10000000M)
        {
            var testCases = new List<TradingCase>();
            for (int i = 1; i <= 100; i++)
            {
                for (int j = 1; j <= 100; j++)
                {
                    if (i > j)
                    {
                        continue;
                    }
                    testCases.Add(new()
                    {
                        TotalTax = 0,
                        ExplainNotes = new(),
                        Profit = 0,
                        FixedCapital = fixedCapital,
                        FirstEmaSell = i,
                        SecondEmaSell = j,
                        FirstEmaBuy = i,
                        SecondEmaBuy = j
                    });
                }
            }
            return testCases.ToArray();
        }
    }
}