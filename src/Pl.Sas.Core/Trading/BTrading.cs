using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class BTrading
    {
        private const float _buyTax = 0.25f / 100;
        private const float SellTax = 0.35f / 100;
        private const float AdvanceTax = 0.35f / 100;
        private static List<MacdResult> _macd_12_26_9 = new();
        private static List<StochRsiResult> _stochRsi_14_14_3_3 = new();
        private static List<EmaResult> _ema_200 = new();
        private static List<EmaResult> _ema_100 = new();
        private static List<EmaResult> _ema_50 = new();
        private static List<EmaResult> _ema_36 = new();
        private static List<EmaResult> _ema_30 = new();
        private static List<EmaResult> _ema_26 = new();
        private static List<EmaResult> _ema_20 = new();
        private static List<EmaResult> _ema_15 = new();
        private static List<EmaResult> _ema_12 = new();
        private static List<EmaResult> _ema_9 = new();
        private static List<EmaResult> _ema_5 = new();
        private static List<EmaResult> _ema_3 = new();
        private static List<ChartPrice> _chartPrices = new();


        public static (bool IsBuy, bool IsSell) Trading(TradingCase tradingCase, List<ChartPrice> chartPrices)
        {
            var numberChangeDay = 10;
            var tradingHistory = new List<ChartPrice>();
            float? lastBuyPrice = null;
            ChartPrice? previousChart = null;

            foreach (var day in chartPrices)
            {
                if (previousChart is null)
                {
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, không giao dịch ngày đầu tiên");
                    previousChart = day;
                    tradingHistory.Add(day);
                    continue;
                }

                if (lastBuyPrice is null)
                {
                    var isBuy = BuyCondition(previousChart.TradingDate);
                    if (isBuy > 0)
                    {
                        var optimalBuyPrice = CalculateOptimalBuyPrice(previousChart);
                        if (optimalBuyPrice <= day.LowestPrice)
                        {
                            optimalBuyPrice = day.ClosePrice;
                        }
                        var (stockCount, excessCash, totalTax) = Buy(tradingCase.TradingMoney, optimalBuyPrice * 1000, numberChangeDay);
                        tradingCase.TradingMoney = excessCash;
                        tradingCase.TotalTax += totalTax;
                        tradingCase.NumberStock += stockCount;
                        lastBuyPrice = optimalBuyPrice;
                        numberChangeDay = 0;
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Mua {tradingCase.NumberStock:0,0} cổ giá {optimalBuyPrice * 1000:0,0} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch.");
                    }
                }
                else
                {
                    if (numberChangeDay > 2)
                    {
                        var isSell = SellCondition(previousChart.TradingDate);
                        if (isSell > 0)
                        {
                            var optimalSellPrice = CalculateOptimalSellPrice(previousChart);
                            if (optimalSellPrice >= day.HighestPrice)
                            {
                                optimalSellPrice = day.ClosePrice;
                            }
                            var (totalProfit, totalTax) = Sell(tradingCase.NumberStock, optimalSellPrice * 1000);
                            tradingCase.TradingMoney = totalProfit;
                            tradingCase.TotalTax += totalTax;
                            var selNumberStock = tradingCase.NumberStock;
                            tradingCase.NumberStock = 0;
                            numberChangeDay = 0;
                            tradingCase.AddNote(optimalSellPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Bán {selNumberStock:0,0} cổ giá {optimalSellPrice * 1000:0,0} thuế {totalTax:0,0}");
                            lastBuyPrice = null;
                        }
                        else
                        {
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch do mới mua {numberChangeDay} ngày");
                    }
                }

                numberChangeDay++;
                previousChart = day;
                tradingHistory.Add(day);
            }
            var lastHistory = chartPrices[^1];
            var todayIsBuy = false;
            var todayIsSell = false;
            if (lastBuyPrice != null)
            {
                todayIsSell = numberChangeDay > 2 && SellCondition(lastHistory.TradingDate) > 0;
            }
            else
            {
                todayIsBuy = BuyCondition(lastHistory.TradingDate) > 0;
            }

            return (todayIsBuy, todayIsSell);
        }

        public static (long StockCount, float ExcessCash, float TotalTax) Buy(float totalMoney, float stockPrice, int numberChangeDay)
        {
            if (stockPrice == 0)
            {
                return (0, totalMoney, 0);
            }

            var buyTax = _buyTax;
            if (numberChangeDay < 3)
            {
                buyTax += AdvanceTax;
            }

            var totalBuyMoney = totalMoney - (totalMoney * buyTax);
            var buyStockCount = (long)Math.Floor(totalBuyMoney / stockPrice);
            if (buyStockCount <= 0)
            {
                return (0, totalMoney, 0);
            }

            var totalValueStock = buyStockCount * stockPrice;
            var totalTax = totalValueStock * buyTax;
            var excessCash = totalMoney - (totalValueStock + totalTax);
            return (buyStockCount, excessCash, totalTax);
        }

        public static int BuyCondition(DateTime tradingDate)
        {
            //var ema200 = _ema_200.Find(tradingDate);
            //var ema100 = _ema_100.Find(tradingDate);

            //if (ema200 is null || ema100 is null || ema100.Ema < ema200.Ema)
            //{
            //    return 0;
            //}


            //var ema50 = _ema_50.Find(tradingDate);
            ////if (ema50 is null || ema50.Ema < ema100.Ema)
            ////{
            ////    return 0;
            ////}

            //var topFiveEma50 = _ema_50.Where(q => q.Date <= tradingDate && q.Ema.HasValue).OrderByDescending(q => q.Date).Take(5);
            //if (topFiveEma50.Count() < 5 || topFiveEma50.Average(q => q.Ema) > ema50.Ema)
            //{
            //    return 0;
            //}

            var rsi = _stochRsi_14_14_3_3.Find(tradingDate);
            if (rsi is null)
            {
                return 0;
            }

            if (rsi.StochRsi > rsi.Signal)
            {
                var macds = _macd_12_26_9.Where(q => q.Date <= tradingDate && q.Macd.HasValue).OrderByDescending(q => q.Date).Take(3).ToList();
                if (macds.Count < 3)
                {
                    return 0;
                }

                if ((macds[0].Macd > macds[1].Macd && macds[1].Macd > macds[2].Macd) || (Math.Abs(macds[0]?.Histogram ?? 0) < Math.Abs(macds[1]?.Histogram ?? 0) && Math.Abs(macds[1]?.Histogram ?? 0) < Math.Abs(macds[2]?.Histogram ?? 0)) || macds[0].Macd > macds[0].Signal)
                {
                    return 100;
                }
            }

            //var ema12 = _ema_12.Find(tradingDate);
            //var ema5 = _ema_5.Find(tradingDate);
            //if (ema5 is null || ema12 is null || ema5.Ema < ema12.Ema)
            //{
            //    return 0;
            //}

            //var ema36 = _ema_36.Find(tradingDate);
            //var ema12 = _ema_12.Find(tradingDate);
            //if (ema36 is null || ema12 is null || ema12.Ema < ema36.Ema)
            //{
            //    return 0;
            //}

            //var ema5 = _ema_5.Find(tradingDate);
            //if (ema5 is null || ema5.Ema < ema12.Ema)
            //{
            //    return 0;
            //}

            //var topThree = _ema_5.Where(q => q.Date <= tradingDate && q.Ema.HasValue).OrderByDescending(q => q.Date).Take(3);
            //if (topThree.Count() < 3)
            //{
            //    return 0;
            //}
            //if (topThree.Average(q => q.Ema) > ema5.Ema)
            //{
            //    return 0;
            //}

            return 0;
        }

        public static float CalculateOptimalBuyPrice(ChartPrice chartPrice)
        {
            var buyPrice = chartPrice.ClosePrice - (chartPrice.ClosePrice / 100 * Math.Abs(chartPrice.LowestPrice.GetPercent(chartPrice.OpenPrice)));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static (float TotalProfit, float TotalTax) Sell(long stockCount, float stockPrice)
        {
            var totalMoney = stockCount * stockPrice;
            var totalTax = totalMoney * SellTax;
            return (totalMoney - totalTax, totalTax);
        }

        public static int SellCondition(DateTime tradingDate)
        {
            var rsi = _stochRsi_14_14_3_3.Find(tradingDate);
            if (rsi is null)
            {
                return 0;
            }

            if (rsi.StochRsi < rsi.Signal)
            {
                var macds = _macd_12_26_9.Where(q => q.Date <= tradingDate && q.Macd.HasValue).OrderByDescending(q => q.Date).Take(3).ToList();
                if (macds.Count < 3)
                {
                    return 0;
                }

                if ((macds[0].Macd < macds[1].Macd && macds[1].Macd < macds[2].Macd) || (Math.Abs(macds[0]?.Histogram ?? 0) < Math.Abs(macds[1]?.Histogram ?? 0) && Math.Abs(macds[1]?.Histogram ?? 0) < Math.Abs(macds[2]?.Histogram ?? 0)) || macds[0].Macd < macds[0].Signal)
                {
                    return 100;
                }
            }

            //var ema36 = _ema_36.Find(tradingDate);
            //var ema12 = _ema_12.Find(tradingDate);
            //if (ema36 is null || ema12 is null || ema12.Ema > ema36.Ema)
            //{
            //    return 0;
            //}

            return 0;
        }

        public static float CalculateOptimalSellPrice(ChartPrice chartPrice)
        {
            var sellPrice = chartPrice.ClosePrice + (chartPrice.ClosePrice / 100 * Math.Abs(chartPrice.OpenPrice.GetPercent(chartPrice.HighestPrice)));
            return (float)Math.Round((decimal)sellPrice, 2);
        }

        public static TradingCase BuildCase(bool isNoteTrading = true)
        {
            return new TradingCase(isNoteTrading);
        }

        public static void BuildIndicatorSet(List<ChartPrice> chartPrices)
        {
            _chartPrices = chartPrices;
            var quotes = chartPrices.Select(q => new Quote()
            {
                Close = (decimal)q.ClosePrice,
                Open = (decimal)q.OpenPrice,
                High = (decimal)q.HighestPrice,
                Low = (decimal)q.LowestPrice,
                Volume = (decimal)q.TotalMatchVol,
                Date = q.TradingDate
            }).OrderBy(q => q.Date).ToList();
            _macd_12_26_9 = quotes.GetMacd(12, 36, 9, CandlePart.Close).ToList();

            _ema_200 = quotes.GetEma(200, CandlePart.Close).ToList();
            _ema_100 = quotes.GetEma(100, CandlePart.Close).ToList();
            _ema_50 = quotes.GetEma(50, CandlePart.Close).ToList();
            _ema_36 = quotes.GetEma(36, CandlePart.Close).ToList();
            _ema_30 = quotes.GetEma(30, CandlePart.Close).ToList();
            _ema_26 = quotes.GetEma(26, CandlePart.Close).ToList();
            _ema_20 = quotes.GetEma(20, CandlePart.Close).ToList();
            _ema_15 = quotes.GetEma(15, CandlePart.Close).ToList();
            _ema_12 = quotes.GetEma(12, CandlePart.Close).ToList();
            _ema_9 = quotes.GetEma(9, CandlePart.Close).ToList();
            _ema_5 = quotes.GetEma(5, CandlePart.Close).ToList();
            _ema_3 = quotes.GetEma(3, CandlePart.Close).ToList();

            _stochRsi_14_14_3_3 = quotes.GetStochRsi(14, 14, 3, 3).ToList();
        }

        public static void ShowIndicator()
        {
            foreach (var chartPrice in _chartPrices)
            {
                var ema200 = _ema_200.Find(chartPrice.TradingDate);
                var ema100 = _ema_100.Find(chartPrice.TradingDate);
                var ema50 = _ema_50.Find(chartPrice.TradingDate);
                var ema36 = _ema_36.Find(chartPrice.TradingDate);
                var ema26 = _ema_26.Find(chartPrice.TradingDate);
                var ema12 = _ema_12.Find(chartPrice.TradingDate);
                var ema9 = _ema_9.Find(chartPrice.TradingDate);
                var ema5 = _ema_5.Find(chartPrice.TradingDate);
                $"{chartPrice.TradingDate:yyy/MM/dd}, ema200: {ema200?.Ema:00.0000}, ema100: {ema100?.Ema:00.0000}, ema50: {ema50?.Ema:00.0000}, ema36: {ema36?.Ema:00.0000}, ema26: {ema26?.Ema:00.0000}, ema12: {ema12?.Ema:00.0000}, ema9: {ema9?.Ema:00.0000}, ema5: {ema5?.Ema:00.0000}".WriteConsole(ConsoleColor.White);

                var macd = _macd_12_26_9.Find(chartPrice.TradingDate);
                $"{chartPrice.TradingDate:yyy/MM/dd}, macd_12_26_9: {macd?.Macd:00.0000} {macd?.Signal:00.0000} {macd?.Histogram:00.0000}".WriteConsole(ConsoleColor.White);
            }
        }
    }
}