using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class SarTrading : BaseTrading
    {
        private static List<ParabolicSarResult> _parabolicSar = new();
        private static TradingCase tradingCase = new();

        public static TradingCase Trading(List<ChartPrice> chartPrices, bool isNoteTrading = true)
        {
            tradingCase.IsNote = isNoteTrading;
            LoadIndicatorSet(chartPrices);
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

                tradingCase.IsBuy = false;
                tradingCase.IsSell = false;

                var buyPercent = tradingHistory.OrderByDescending(q => q.TradingDate).Take(10).Select(q => Math.Abs(q.OpenPrice.GetPercent(q.HighestPrice))).Average() / 100;
                tradingCase.BuyPrice = day.OpenPrice - (float)(day.OpenPrice * buyPercent);
                var sellPercent = chartPrices.OrderByDescending(q => q.TradingDate).Take(3).Select(q => Math.Abs(q.OpenPrice.GetPercent(q.LowestPrice))).Average() / 100;
                tradingCase.SellPrice = day.OpenPrice + (float)(day.OpenPrice * sellPercent);

                if (lastBuyPrice is null)
                {
                    tradingCase.IsBuy = BuyCondition(day.TradingDate) > 0;
                    if (tradingCase.IsBuy)
                    {
                        if (tradingCase.BuyPrice <= day.LowestPrice)
                        {
                            tradingCase.BuyPrice = day.ClosePrice;
                        }
                        var (stockCount, excessCash, totalTax) = Buy(tradingCase.TradingMoney, tradingCase.BuyPrice * 1000, numberChangeDay);
                        tradingCase.TradingMoney = excessCash;
                        tradingCase.TotalTax += totalTax;
                        tradingCase.NumberStock += stockCount;
                        lastBuyPrice = tradingCase.BuyPrice;
                        numberChangeDay = 0;
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Mua {tradingCase.NumberStock:0,0} cổ giá {tradingCase.BuyPrice * 1000:0,0} thuế {totalTax:0,0}");
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
                        tradingCase.IsSell = SellCondition(day.TradingDate) > 0;
                        if (tradingCase.IsSell)
                        {
                            if (tradingCase.SellPrice >= day.HighestPrice)
                            {
                                tradingCase.SellPrice = day.ClosePrice;
                            }
                            var (totalProfit, totalTax) = Sell(tradingCase.NumberStock, tradingCase.SellPrice * 1000);
                            tradingCase.TradingMoney = totalProfit;
                            tradingCase.TotalTax += totalTax;
                            var selNumberStock = tradingCase.NumberStock;
                            tradingCase.NumberStock = 0;
                            numberChangeDay = 0;
                            tradingCase.AddNote(tradingCase.SellPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Bán {selNumberStock:0,0} cổ giá {tradingCase.SellPrice * 1000:0,0} thuế {totalTax:0,0}");
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

                previousChart = day;
                numberChangeDay++;
                tradingHistory.Add(day);
            }
            return tradingCase;
        }

        public static int BuyCondition(DateTime tradingDate)
        {
            var sar = _parabolicSar.Find(tradingDate);
            if (sar is null || sar.Sar is null)
            {
                return 0;
            }

            if (sar.IsReversal == true)
            {
                return 100;
            }

            return 0;
        }

        public static int SellCondition(DateTime tradingDate)
        {
            var sar = _parabolicSar.Find(tradingDate);
            if (sar is null || sar.Sar is null)
            {
                return 0;
            }

            if (sar.IsReversal == true)
            {
                return 100;
            }

            return 0;
        }

        private static void LoadIndicatorSet(List<ChartPrice> chartPrices)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _parabolicSar = quotes.GetParabolicSar(0.02M).ToList();
        }

        public static void Dispose()
        {
            _parabolicSar = new();
            tradingCase = new();
        }
    }
}