using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    /// <summary>
    /// Trading thử nghiệm
    /// </summary>
    public class ExperimentTrading : BaseTrading
    {
        private static List<MacdResult> _macd_12_26_9 = new();
        private static TradingCase tradingCase = new();

        public static TradingCase Trading(List<ChartPrice> chartPrices, bool isNoteTrading = true)
        {
            tradingCase.IsNote = isNoteTrading;
            var numberChangeDay = 10;
            var tradingHistory = new List<ChartPrice>();
            float? lastBuyPrice = null;
            ChartPrice? previousChart = null;

            foreach (var day in chartPrices)
            {
                if (previousChart is null)
                {
                    tradingCase.AssetPosition = "100% Tiền";
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, không giao dịch ngày đầu tiên");
                    previousChart = day;
                    tradingHistory.Add(day);
                    continue;
                }

                tradingCase.IsBuy = false;
                tradingCase.IsSell = false;
                tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, previousChart.ClosePrice);
                tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, previousChart.ClosePrice);

                if (lastBuyPrice is null)
                {
                    tradingCase.IsBuy = BuyCondition(previousChart.TradingDate) > 0;
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
                        tradingCase.AssetPosition = $"Mua (B:{tradingCase.BuyPrice * 1000:0,0}|S:{tradingCase.SellPrice * 1000:0,0})";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Mua {tradingCase.NumberStock:0,0} cổ giá {tradingCase.BuyPrice * 1000:0,0} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        tradingCase.AssetPosition = $"100% tiền (B:{tradingCase.BuyPrice * 1000:0,0}|S:{tradingCase.SellPrice * 1000:0,0})";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch.");
                    }
                }
                else
                {
                    if (numberChangeDay > 2)
                    {
                        tradingCase.IsSell = SellCondition(previousChart.TradingDate) > 0;
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
                            tradingCase.AssetPosition = $"Bán (B:{tradingCase.BuyPrice * 1000:0,0}|S:{tradingCase.SellPrice * 1000:0,0})";
                            tradingCase.AddNote(tradingCase.SellPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Bán {selNumberStock:0,0} cổ giá {tradingCase.SellPrice * 1000:0,0} ({tradingCase.SellPrice.GetPercent(lastBuyPrice.Value):0,0.00}%) thuế {totalTax:0,0}");
                            lastBuyPrice = null;
                        }
                        else
                        {
                            tradingCase.AssetPosition = $"100% Cổ phiếu (B:{tradingCase.BuyPrice * 1000:0,0}|S:{tradingCase.SellPrice * 1000:0,0})";
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AssetPosition = $"100% Cổ phiếu (B:{tradingCase.BuyPrice * 1000:0,0}|S:{tradingCase.SellPrice * 1000:0,0})";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch do mới mua {numberChangeDay} ngày");
                    }
                }

                previousChart = day;
                numberChangeDay++;
                tradingHistory.Add(day);
            }

            tradingCase.IsBuy = false;
            tradingCase.IsSell = false;
            tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, tradingHistory[^1].ClosePrice);
            tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, tradingHistory[^1].ClosePrice);

            if (tradingCase.NumberStock > 0)
            {
                tradingCase.IsSell = SellCondition(tradingHistory[^1].TradingDate) > 0;
            }
            else
            {
                tradingCase.IsBuy = BuyCondition(tradingHistory[^1].TradingDate) > 0;
            }

            _macd_12_26_9 = new List<MacdResult>();
            return tradingCase;
        }

        public static int BuyCondition(DateTime tradingDate)
        {
            var macd = _macd_12_26_9.Find(tradingDate);
            if (macd is null || macd.Macd is null)
            {
                return 0;
            }

            if (macd.Macd > macd.Signal)
            {
                return 100;
            }

            return 0;
        }

        public static int SellCondition(DateTime tradingDate)
        {
            var macd = _macd_12_26_9.Find(tradingDate);
            if (macd is null || macd.Macd is null)
            {
                return 0;
            }

            if (macd.Macd < macd.Signal)
            {
                return 100;
            }

            return 0;
        }

        public static float CalculateOptimalBuyPrice(List<ChartPrice> chartPrices, float openPriceToday)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(30).Select(q => Math.Abs(q.ClosePrice.GetPercent(q.HighestPrice))).Average() / 100;
            var buyPrice = openPriceToday - (openPriceToday * percent);
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float openPriceToday)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(30).Select(q => Math.Abs(q.ClosePrice.GetPercent(q.LowestPrice))).Average() / 100;
            var buyPrice = openPriceToday + (openPriceToday * percent);
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static void LoadIndicatorSet(List<ChartPrice> chartPrices)
        {
            var quotes = chartPrices.Select(q => new Quote()
            {
                Close = (decimal)q.ClosePrice,
                Open = (decimal)q.OpenPrice,
                High = (decimal)q.HighestPrice,
                Low = (decimal)q.LowestPrice,
                Volume = (decimal)q.TotalMatchVol,
                Date = q.TradingDate
            }).OrderBy(q => q.Date).ToList();
            _macd_12_26_9 = quotes.GetMacd(12, 26, 9, CandlePart.Close).ToList();
        }

        public static void Dispose()
        {
            _macd_12_26_9 = new();
            tradingCase = new();
        }
    }
}