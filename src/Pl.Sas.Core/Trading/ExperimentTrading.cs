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

        public static TradingCase Trading(List<ChartPrice> chartPrices, List<ChartPrice> tradingHistory, string exchangeName, bool isNoteTrading = true)
        {
            tradingCase = new() { IsNote = isNoteTrading };
            var lastTradingDate = chartPrices[^1].TradingDate;

            foreach (var day in chartPrices)
            {
                if (tradingHistory.Count <= 0)
                {
                    tradingCase.AssetPosition = "100% T";
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, không giao dịch ngày đầu tiên");
                    tradingHistory.Add(day);
                    continue;
                }

                var isRealtime = day.TradingDate == lastTradingDate;
                var timeTrading = GetTimeTrading(exchangeName, DateTime.Now);

                if (tradingCase.NumberStock <= 0)
                {
                    tradingCase.IsBuy = BuyCondition(day.TradingDate) > 0;
                    if (tradingCase.IsBuy)
                    {
                        tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, day.OpenPrice);
                        if (isRealtime && timeTrading != TimeTrading.DON)
                        {
                            if (timeTrading == TimeTrading.NST)
                            {
                                tradingCase.AssetPosition = $"100% T";
                                tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Chưa đến giờ giao dịch.");
                            }
                            else if (timeTrading == TimeTrading.ATC && tradingCase.BuyPrice <= day.LowestPrice)
                            {
                                tradingCase.AssetPosition = $"M: ATC";
                                tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Đặt lệnh mua ATC.");
                            }
                            else if (timeTrading == TimeTrading.TMP && tradingCase.BuyPrice <= day.LowestPrice)
                            {
                                tradingCase.AssetPosition = $"M: MP";
                                tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Đặt lệnh mua MP.");
                            }
                            else
                            {
                                tradingCase.AssetPosition = $"M: LO({tradingCase.BuyPrice:0,0.00})";
                                tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Đặt lệnh mua LO với giá {tradingCase.BuyPrice:0,0.00}.");
                            }
                        }
                        else
                        {
                            if (tradingCase.BuyPrice <= day.LowestPrice)
                            {
                                tradingCase.BuyPrice = day.ClosePrice;
                                tradingCase.NumberPriceClose++;
                            }
                            else
                            {
                                tradingCase.NumberPriceNeed++;
                            }
                            var (stockCount, excessCash, totalTax) = Buy(tradingCase.TradingMoney, tradingCase.BuyPrice * 1000, tradingCase.NumberChangeDay);
                            tradingCase.TradingMoney = excessCash;
                            tradingCase.TotalTax += totalTax;
                            tradingCase.NumberStock += stockCount;
                            tradingCase.NumberChangeDay = 0;
                            tradingCase.AssetPosition = $"M: {tradingCase.BuyPrice:0,0.00}";
                            tradingCase.ActionPrice = tradingCase.BuyPrice;
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Mua {tradingCase.NumberStock:0,0} cổ giá {tradingCase.BuyPrice * 1000:0,0} thuế {totalTax:0,0}");
                        }
                    }
                    else
                    {
                        tradingCase.AssetPosition = "100% T";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch.");
                    }
                }
                else
                {
                    if (tradingCase.NumberChangeDay > 2)
                    {
                        tradingCase.IsSell = SellCondition(day.TradingDate) > 0;
                        if (tradingCase.IsSell)
                        {
                            tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, day.OpenPrice);
                            if (isRealtime && timeTrading != TimeTrading.DON)
                            {
                                if (timeTrading == TimeTrading.NST)
                                {
                                    tradingCase.AssetPosition = $"100% C";
                                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Chưa đến giờ giao dịch.");
                                }
                                else if (timeTrading == TimeTrading.ATC && tradingCase.SellPrice >= day.HighestPrice)
                                {
                                    tradingCase.AssetPosition = $"B: ATC";
                                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Đặt lệnh mua ATC.");
                                }
                                else if (timeTrading == TimeTrading.TMP && tradingCase.SellPrice >= day.HighestPrice)
                                {
                                    tradingCase.AssetPosition = $"B: MP";
                                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Đặt lệnh mua MP.");
                                }
                                else
                                {
                                    tradingCase.AssetPosition = $"B: LO({tradingCase.SellPrice:0,0.00})";
                                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Đặt lệnh mua LO với giá {tradingCase.SellPrice:0,0.00}.");
                                }
                            }
                            else
                            {
                                if (tradingCase.SellPrice >= day.HighestPrice)
                                {
                                    tradingCase.SellPrice = day.ClosePrice;
                                    tradingCase.NumberPriceClose++;
                                }
                                else
                                {
                                    tradingCase.NumberPriceNeed++;
                                }
                                var (totalProfit, totalTax) = Sell(tradingCase.NumberStock, tradingCase.SellPrice * 1000);
                                tradingCase.TradingMoney = totalProfit;
                                tradingCase.TotalTax += totalTax;
                                var selNumberStock = tradingCase.NumberStock;
                                tradingCase.NumberStock = 0;
                                tradingCase.NumberChangeDay = 0;
                                tradingCase.AssetPosition = $"B: {tradingCase.ActionPrice:0,0.00}";
                                tradingCase.AddNote(tradingCase.SellPrice > tradingCase.ActionPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Bán {selNumberStock:0,0} cổ giá {tradingCase.SellPrice * 1000:0,0} ({tradingCase.SellPrice.GetPercent(tradingCase.ActionPrice):0,0.00}%) thuế {totalTax:0,0}");
                                tradingCase.ActionPrice = tradingCase.SellPrice;
                            }
                        }
                        else
                        {
                            tradingCase.AssetPosition = "100% C";
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AssetPosition = "100% C";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch do mới mua {tradingCase.NumberChangeDay} ngày");
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

            var topThree = _macd_12_26_9.Where(q => q.Date <= tradingDate).OrderByDescending(q => q.Date).Take(3).ToList();
            if (topThree.Count != 3 || topThree[0].Histogram is null || topThree[1].Histogram is null || topThree[2].Histogram is null)
            {
                return 0;
            }

            if (topThree[0].Histogram > topThree[1].Histogram && topThree[1].Histogram > topThree[2].Histogram)
            {
                var avgValue = ((topThree[0].Histogram - topThree[1].Histogram) + (topThree[1].Histogram - topThree[2].Histogram)) / 2;
                if (avgValue > -topThree[0].Histogram)
                {
                    return 100;
                }
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

            var topThree = _macd_12_26_9.Where(q => q.Date <= tradingDate).OrderByDescending(q => q.Date).Take(3).ToList();
            if (topThree.Count != 3 || topThree[0].Histogram is null || topThree[1].Histogram is null || topThree[2].Histogram is null)
            {
                return 0;
            }

            if (topThree[0].Histogram < topThree[1].Histogram && topThree[1].Histogram < topThree[2].Histogram)
            {
                var avgValue = ((topThree[0].Histogram - topThree[1].Histogram) + (topThree[1].Histogram - topThree[2].Histogram)) / 2;
                if (-avgValue > topThree[0].Histogram)
                {
                    return 100;
                }
            }

            return 0;
        }

        public static float CalculateOptimalBuyPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.HighestPrice.GetPercent(q.OpenPrice)).Average() / 100;
            var buyPrice = rootPrice - (rootPrice * (percent / 10));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.OpenPrice.GetPercent(q.LowestPrice)).Average() / 100;
            var buyPrice = rootPrice + (rootPrice * (percent / 10));
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