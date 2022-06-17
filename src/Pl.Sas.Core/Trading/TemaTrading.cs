using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    /// <summary>
    /// Trading thử nghiệm
    /// </summary>
    public class TemaTrading : BaseTrading
    {
        private List<TemaResult> _tema12 = new();
        private List<TemaResult> _tema26 = new();
        private List<TemaResult> _tema50 = new();
        private TradingCase tradingCase = new();

        public TemaTrading(List<ChartPrice> chartPrices)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _tema12 = quotes.GetTema(12).ToList();
            _tema26 = quotes.GetTema(26).ToList();
            _tema50 = quotes.GetTema(100).ToList();
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
                tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, day.OpenPrice);
                tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, day.OpenPrice);
                var timeTrading = GetTimeTrading(exchangeName, DateTime.Now);

                if (tradingCase.NumberStock <= 0)
                {
                    tradingCase.IsBuy = BuyCondition(tradingHistory.Last().TradingDate) > 0;
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
                        tradingCase.IsSell = SellCondition(tradingHistory.Last().TradingDate) > 0;
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

            _tema12 = new List<TemaResult>();
            _tema26 = new List<TemaResult>();
            _tema50 = new List<TemaResult>();
            return tradingCase;
        }

        public int BuyCondition(DateTime tradingDate)
        {
            var tema12 = _tema12.Find(tradingDate);
            var tema26 = _tema26.Find(tradingDate);
            var tema50 = _tema50.Find(tradingDate);

            if (tema12 is null || tema12.Tema is null || tema26 is null || tema26.Tema is null || tema50 is null || tema50.Tema is null)
            {
                return 0;
            }

            if (tema12.Tema < tema50.Tema)
            {
                return 0;
            }

            if (tema12.Tema < tema26.Tema)
            {
                return 0;
            }

            //var topThree = _tema26.Where(q => q.Date <= tradingDate).OrderByDescending(q => q.Date).Take(3).ToList();
            //if (topThree.Count != 3 || topThree[0].Tema is null || topThree[1].Tema is null || topThree[2].Tema is null)
            //{
            //    return 0;
            //}

            //if (topThree[0].Tema > topThree[1].Tema)
            //{
            //    var sub = topThree[0].Tema - topThree[1].Tema;
            //    if (topThree[0].Tema + sub >= tema26.Tema)
            //    {
            //        return 100;
            //    }
            //}

            return 100;
        }

        public int SellCondition(DateTime tradingDate)
        {
            var tema12 = _tema12.Find(tradingDate);
            var tema26 = _tema26.Find(tradingDate);
            if (tema12 is null || tema12.Tema is null || tema26 is null || tema26.Tema is null)
            {
                return 100;
            }

            if (tema12.Tema < tema26.Tema)
            {
                return 100;
            }

            //var topThree = _tema26.Where(q => q.Date <= tradingDate).OrderByDescending(q => q.Date).Take(3).ToList();
            //if (topThree.Count != 3 || topThree[0].Tema is null || topThree[1].Tema is null || topThree[2].Tema is null)
            //{
            //    return 0;
            //}

            //if (topThree[0].Tema > topThree[1].Tema)
            //{
            //    var sub = topThree[0].Tema - topThree[1].Tema;
            //    if (topThree[0].Tema + sub <= tema26.Tema)
            //    {
            //        return 100;
            //    }
            //}

            return 0;
        }

        public static float CalculateOptimalBuyPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.HighestPrice.GetPercent(q.OpenPrice)).Average() / 100;
            var buyPrice = rootPrice - (rootPrice * (percent / 2));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.OpenPrice.GetPercent(q.LowestPrice)).Average() / 100;
            var buyPrice = rootPrice + (rootPrice * (percent / 2));
            return (float)Math.Round((decimal)buyPrice, 2);
        }
    }
}