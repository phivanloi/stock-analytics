using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class FiveEmaTrading : BaseTrading
    {
        private readonly List<DemaResult> _verySlowDemas;
        private readonly List<DemaResult> _veryFastDemas;
        private readonly List<EmaResult> _slowEmas;
        private readonly List<EmaResult> _fastEmas;
        private readonly List<RsiResult> _fastRsis;
        private readonly List<RsiResult> _slowRsis;
        private readonly TradingCase tradingCase = new();

        public FiveEmaTrading(List<ChartPrice> chartPrices)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _fastEmas = quotes.Use(CandlePart.Close).GetEma(12).ToList();
            _slowEmas = quotes.Use(CandlePart.Close).GetEma(28).ToList();
            _veryFastDemas = quotes.Use(CandlePart.Close).GetDema(3).ToList();
            _verySlowDemas = quotes.Use(CandlePart.Close).GetDema(12).ToList();
            _fastRsis = quotes.GetRsi(1).ToList();
            _slowRsis = quotes.GetRsi(14).ToList();
        }

        public TradingCase Trading(List<ChartPrice> chartPrices, List<ChartPrice> tradingHistory, string exchangeName)
        {
              foreach (var day in chartPrices)
            {
                if (tradingHistory.Count <= 0)
                {
                    tradingCase.AssetPosition = $"T-{tradingCase.NumberChangeDay}";
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> không giao dịch.");
                    tradingHistory.Add(day);
                    continue;
                }

                RebuildStatus(tradingHistory[^1]);
                tradingCase.IsBuy = false;
                tradingCase.IsSell = false;
                var timeTrading = GetTimeTrading(exchangeName, DateTime.Now);

                if (tradingCase.NumberStock <= 0)
                {
                    tradingCase.IsBuy = BuyCondition(tradingHistory[^1].TradingDate) > 0 && tradingCase.ContinueBuy;
                    if (tradingCase.IsBuy)
                    {
                        tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, day.OpenPrice);
                        tradingCase.ActionPrice = tradingCase.BuyPrice;
                        if (tradingCase.ActionPrice <= day.LowestPrice)
                        {
                            tradingCase.ActionPrice = day.ClosePrice;
                            tradingCase.NumberPriceClose++;
                        }
                        else
                        {
                            tradingCase.NumberPriceNeed++;
                        }
                        var (stockCount, excessCash, totalTax) = Buy(tradingCase.TradingMoney, tradingCase.ActionPrice * 1000, tradingCase.NumberChangeDay);
                        tradingCase.TradingMoney = excessCash;
                        tradingCase.TotalTax += totalTax;
                        tradingCase.NumberStock += stockCount;
                        tradingCase.NumberChangeDay = 0;
                        tradingCase.MaxPriceOnBuy = day.ClosePrice;
                        tradingCase.StopLossPrice = tradingCase.ActionPrice - (tradingCase.ActionPrice * 0.07f);
                        if (timeTrading == TimeTrading.NST || DateTime.Now.Date != day.TradingDate)
                        {
                            tradingCase.AssetPosition = $"C-{tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(tradingCase.ActionPrice):0.0}";
                        }
                        else
                        {
                            if (timeTrading == TimeTrading.DON)
                            {
                                tradingCase.AssetPosition = $"C-{tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(tradingCase.ActionPrice):0.0}";
                            }
                            else
                            {
                                tradingCase.AssetPosition = $"Mua";
                            }
                        }
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Mua {tradingCase.NumberStock:0,0} cổ giá {tradingCase.ActionPrice:0,0.00} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        tradingCase.AssetPosition = $"T-{tradingCase.NumberChangeDay}";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Không giao dịch.");
                    }
                }
                else
                {
                    if (tradingCase.NumberChangeDay > _timeStockCome)
                    {
                        tradingCase.IsSell = SellCondition(tradingHistory[^1].TradingDate, tradingHistory[^1].ClosePrice) > 0;
                        if (tradingCase.IsSell)
                        {
                            tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, day.OpenPrice);
                            var lastBuyPrice = tradingCase.ActionPrice;
                            tradingCase.ActionPrice = tradingCase.SellPrice;
                            if (tradingCase.ActionPrice >= day.HighestPrice)
                            {
                                tradingCase.ActionPrice = day.ClosePrice;
                                tradingCase.NumberPriceClose++;
                            }
                            else
                            {
                                tradingCase.NumberPriceNeed++;
                            }
                            var (totalProfit, totalTax) = Sell(tradingCase.NumberStock, tradingCase.ActionPrice * 1000);
                            tradingCase.TradingMoney = totalProfit;
                            tradingCase.TotalTax += totalTax;
                            var selNumberStock = tradingCase.NumberStock;
                            tradingCase.NumberStock = 0;
                            tradingCase.NumberChangeDay = 0;
                            if (timeTrading == TimeTrading.NST || DateTime.Now.Date != day.TradingDate)
                            {
                                tradingCase.AssetPosition = $"T-{tradingCase.NumberChangeDay}";
                            }
                            else
                            {
                                if (timeTrading == TimeTrading.DON)
                                {
                                    tradingCase.AssetPosition = $"T-{tradingCase.NumberChangeDay}";
                                }
                                else
                                {
                                    tradingCase.AssetPosition = $"Bán";
                                }
                            }
                            tradingCase.AddNote(tradingCase.ActionPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Bán {selNumberStock:0,0} cổ giá {tradingCase.ActionPrice:0,0.00} ({tradingCase.ActionPrice.GetPercent(lastBuyPrice):0,0.00}%), Max: ({tradingCase.MaxPriceOnBuy.GetPercent(tradingCase.ActionPrice):0,0.00}%) thuế {totalTax:0,0}");
                        }
                        else
                        {
                            tradingCase.AssetPosition = $"C-{tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(tradingCase.ActionPrice):0.0}";
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AssetPosition = $"C-{tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(tradingCase.ActionPrice):0.0}";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Không giao dịch do mới mua {tradingCase.NumberChangeDay} ngày");
                    }
                }

                tradingCase.NumberChangeDay++;
                if (tradingCase.NumberStock <= 0)
                {
                    tradingCase.NumberDayInMoney++;
                }
                else
                {
                    tradingCase.NumberDayInStock++;
                }
                tradingHistory.Add(day);
            }

            return tradingCase;
        }

        public void RebuildStatus(ChartPrice chartPrice)
        {
            if (tradingCase.MaxPriceOnBuy < chartPrice.ClosePrice)
            {
                tradingCase.MaxPriceOnBuy = chartPrice.ClosePrice;//Đặt lại giá cao nhất đã đạt được
            }

            var slowEma = _slowEmas.Find(chartPrice.TradingDate);
            if (slowEma is null || slowEma.Ema is null)
            {
                return;
            }

            var fastEma = _fastEmas.Find(chartPrice.TradingDate);
            if (fastEma is null || fastEma.Ema is null)
            {
                return;
            }

            if (fastEma.Ema < slowEma.Ema && !tradingCase.ContinueBuy)
            {
                var verySlowEma = _verySlowDemas.Find(chartPrice.TradingDate);
                if (verySlowEma is null || verySlowEma.Dema is null)
                {
                    return;
                }

                var veryFastEma = _veryFastDemas.Find(chartPrice.TradingDate);
                if (veryFastEma is null || veryFastEma.Dema is null)
                {
                    return;
                }

                if (veryFastEma.Dema < verySlowEma.Dema && !tradingCase.ContinueBuy)
                {
                    tradingCase.AddNote(0, $"{chartPrice.TradingDate:yy/MM/dd}: Cho phép lệnh mua được hoạt động do đường VeryFastEma đã cắt xuống đường VerySlowEma.");
                    tradingCase.ContinueBuy = true;
                }
            }
        }

        public int BuyCondition(DateTime tradingDate)
        {
            var slowEma = _slowEmas.Find(tradingDate);
            if (slowEma is null || slowEma.Ema is null)
            {
                return 0;
            }

            var fastEma = _fastEmas.Find(tradingDate);
            if (fastEma is null || fastEma.Ema is null)
            {
                return 0;
            }

            if (fastEma.Ema < slowEma.Ema)
            {
                return 0;
            }

            var verySlowEma = _verySlowDemas.Find(tradingDate);
            if (verySlowEma is null || verySlowEma.Dema is null)
            {
                return 0;
            }

            var veryFastEma = _veryFastDemas.Find(tradingDate);
            if (veryFastEma is null || veryFastEma.Dema is null)
            {
                return 0;
            }

            if (veryFastEma.Dema < verySlowEma.Dema)
            {
                return 0;
            }

            var slowRsi = _slowRsis.Find(tradingDate);
            if (slowRsi is null || slowRsi.Rsi is null)
            {
                return 0;
            }

            var fastRsi = _fastRsis.Find(tradingDate);
            if (fastRsi is null || fastRsi.Rsi is null)
            {
                return 0;
            }

            if (fastRsi.Rsi < slowRsi.Rsi)
            {
                return 0;
            }

            return 100;
        }

        public int SellCondition(DateTime tradingDate, float lastClosePrice)
        {
            if (lastClosePrice <= tradingCase.StopLossPrice)
            {
                tradingCase.AddNote(-1, $"{tradingDate:yy/MM/dd}: Kích hoạt lệnh bán chặn lỗ, giá mua {tradingCase.ActionPrice:0.0,00} giá kích hoạt {lastClosePrice:0.0,00}({lastClosePrice.GetPercent(tradingCase.ActionPrice):0.0,00})");
                tradingCase.ContinueBuy = false;
                return 100;
            }

            var verySlowEma = _verySlowDemas.Find(tradingDate);
            if (verySlowEma is null || verySlowEma.Dema is null)
            {
                return 0;
            }

            var veryFastEma = _veryFastDemas.Find(tradingDate);
            if (veryFastEma is null || veryFastEma.Dema is null)
            {
                return 0;
            }

            if (veryFastEma.Dema > verySlowEma.Dema)
            {
                return 0;
            }

            var slowRsi = _slowRsis.Find(tradingDate);
            if (slowRsi is null || slowRsi.Rsi is null)
            {
                return 0;
            }

            var fastRsi = _fastRsis.Find(tradingDate);
            if (fastRsi is null || fastRsi.Rsi is null)
            {
                return 0;
            }

            if (fastRsi.Rsi > slowRsi.Rsi)
            {
                return 0;
            }

            return 100;
        }

        public static float CalculateOptimalBuyPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.HighestPrice.GetPercent(q.OpenPrice)).Average() / 100;
            var buyPrice = rootPrice - (rootPrice * (percent * 5));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.OpenPrice.GetPercent(q.LowestPrice)).Average() / 100;
            var buyPrice = rootPrice + (rootPrice * (percent * 5));
            return (float)Math.Round((decimal)buyPrice, 2);
        }
    }
}