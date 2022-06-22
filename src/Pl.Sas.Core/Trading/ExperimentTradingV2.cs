using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    /// <summary>
    /// Trading thử nghiệm
    /// </summary>
    public class ExperimentTradingV2 : BaseTrading
    {
        private readonly List<SmaResult> _sma_10 = new();
        private readonly List<SmaResult> _sma_1 = new();
        private readonly List<SmaResult> _sma_50 = new();
        private readonly List<ZigZagResult> _zigZag = new();
        private TradingCase tradingCase = new();

        public ExperimentTradingV2(List<ChartPrice> chartPrices, List<ChartPrice> indexChartPrices)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var indexQuotes = indexChartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _sma_10 = quotes.Use(CandlePart.Close).GetSma(10).ToList();
            _sma_1 = quotes.Use(CandlePart.Close).GetSma(1).ToList();
            _sma_50 = quotes.Use(CandlePart.Close).GetSma(50).ToList();
            _zigZag = quotes.GetZigZag(EndType.HighLow, 5).ToList();
        }

        public TradingCase Trading(List<ChartPrice> chartPrices, List<ChartPrice> tradingHistory, string exchangeName, bool isNoteTrading = true)
        {
            tradingCase = new() { IsNote = isNoteTrading };
            foreach (var day in chartPrices)
            {
                if (tradingHistory.Count <= 0)
                {
                    tradingCase.AssetPosition = "100% T";
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> không giao dịch ngày đầu tiên");
                    tradingHistory.Add(day);
                    continue;
                }

                tradingCase.IsBuy = false;
                tradingCase.IsSell = false;
                tradingCase.BuyPrice = day.ClosePrice; //CalculateOptimalBuyPrice(tradingHistory, day.OpenPrice);
                tradingCase.SellPrice = day.ClosePrice;//CalculateOptimalSellPrice(tradingHistory, day.OpenPrice);
                var timeTrading = GetTimeTrading(exchangeName, DateTime.Now);

                if (tradingCase.NumberStock <= 0)
                {
                    tradingCase.IsBuy = BuyCondition(day.TradingDate, tradingHistory.Last().ClosePrice) > 0;
                    if (tradingCase.IsBuy)
                    {
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
                        if (timeTrading == TimeTrading.NST || DateTime.Now.Date != day.TradingDate)
                        {
                            tradingCase.AssetPosition = "100% C";
                        }
                        else
                        {
                            tradingCase.AssetPosition = $"M:({tradingCase.BuyPrice:0,0.00})";
                        }
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Mua {tradingCase.NumberStock:0,0} cổ giá {tradingCase.ActionPrice:0,0.00} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        tradingCase.AssetPosition = "100% T";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}|-> Không giao dịch.");
                    }
                }
                else
                {
                    if (tradingCase.NumberChangeDay > 2)
                    {
                        tradingCase.IsSell = SellCondition(day.TradingDate) > 0;
                        if (tradingCase.IsSell)
                        {
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
                                tradingCase.AssetPosition = "100% T";
                            }
                            else
                            {
                                tradingCase.AssetPosition = $"B:({tradingCase.SellPrice:0,0.00})";
                            }
                            tradingCase.AddNote(tradingCase.ActionPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Bán {selNumberStock:0,0} cổ giá {tradingCase.ActionPrice:0,0.00} ({tradingCase.ActionPrice.GetPercent(lastBuyPrice):0,0.00}%) thuế {totalTax:0,0}");
                        }
                        else
                        {
                            tradingCase.AssetPosition = "100% C";
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AssetPosition = "100% C";
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

        public int BuyCondition(DateTime tradingDate, float lastClosePrice)
        {
            //var zigZagResultH = _zigZag.LastOrDefault(q => q.Date <= tradingDate && q.PointType == "H" && ((decimal)lastClosePrice < (q.ZigZag - (q.ZigZag * 0.01m))));
            //var zigZagResultL = _zigZag.LastOrDefault(q => q.Date <= tradingDate && q.PointType == "L" && ((decimal)lastClosePrice > (q.ZigZag + (q.ZigZag * 0.01m))));

            //if (zigZagResultH is not null && zigZagResultH.ZigZag is not null && zigZagResultL is not null && zigZagResultL.ZigZag is not null)
            //{
            //    var percentDown = ((float)zigZagResultL.ZigZag.Value).GetPercent(lastClosePrice);
            //    var percetUp = lastClosePrice.GetPercent((float)zigZagResultH.ZigZag.Value);
            //    if (percetUp <= percentDown)
            //    {
            //        return 0;
            //    }
            //}

            var sma1 = _sma_1.Find(tradingDate);
            if (sma1 is null || sma1.Sma is null)
            {
                return 0;
            }

            var sma50 = _sma_50.Find(tradingDate);
            if (sma50 is null || sma50.Sma is null)
            {
                return 0;
            }

            if (sma1.Sma < sma50.Sma)
            {
                return 0;
            }

            var sma9 = _sma_10.Find(tradingDate);
            if (sma9 is null || sma9.Sma is null)
            {
                return 0;
            }

            if (sma1.Sma < sma9.Sma)
            {
                return 0;
            }

            return 100;
        }

        public int SellCondition(DateTime tradingDate)
        {
            var sma9 = _sma_10.Find(tradingDate);
            if (sma9 is null || sma9.Sma is null)
            {
                return 0;
            }

            var sma1 = _sma_1.Find(tradingDate);
            if (sma1 is null || sma1.Sma is null)
            {
                return 0;
            }

            if (sma1.Sma > sma9.Sma)
            {
                return 0;
            }

            return 100;
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
    }
}