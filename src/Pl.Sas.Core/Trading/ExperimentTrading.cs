using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class ExperimentTrading : BaseTrading
    {
        private readonly List<SmaResult> _sma_10;
        private readonly List<SmaResult> _sma_6;
        private readonly List<SmaResult> _sma_36;
        private readonly List<StochRsiResult> _stochRsi_14_14_3_3;
        private TradingCase tradingCase = new();

        public ExperimentTrading(List<ChartPrice> chartPrices, List<ChartPrice> indexChartPrices)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var indexQuotes = indexChartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _sma_10 = quotes.Use(CandlePart.Close).GetSma(10).ToList();
            _sma_6 = quotes.Use(CandlePart.Close).GetSma(6).ToList();
            _sma_36 = quotes.Use(CandlePart.Close).GetSma(36).ToList();
            _stochRsi_14_14_3_3 = quotes.GetStochRsi(14, 14, 3, 3).ToList();
        }

        public TradingCase Trading(List<ChartPrice> chartPrices, List<ChartPrice> tradingHistory, string exchangeName, bool isNoteTrading = true)
        {
            tradingCase = new() { IsNote = isNoteTrading, StopLossPercent = -7f };

            foreach (var day in chartPrices)
            {
                if (tradingHistory.Count <= 0)
                {
                    tradingCase.AssetPosition = "100% T";
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> không giao dịch ngày đầu tiên");
                    tradingHistory.Add(day);
                    continue;
                }

                RebuildStatus(tradingHistory.Last());

                tradingCase.IsBuy = false;
                tradingCase.IsSell = false;
                tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, day.OpenPrice);
                tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, day.OpenPrice);
                var timeTrading = GetTimeTrading(exchangeName, DateTime.Now);

                if (tradingCase.NumberStock <= 0)
                {
                    tradingCase.IsBuy = BuyCondition(tradingHistory.Last().TradingDate, tradingHistory.Last().ClosePrice) > 0 && tradingCase.ContinueBuy;
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
                        tradingCase.MaxPriceOnBuy = day.ClosePrice;
                        if (timeTrading == TimeTrading.NST || DateTime.Now.Date != day.TradingDate)
                        {
                            tradingCase.AssetPosition = "100% C";
                        }
                        else
                        {
                            tradingCase.AssetPosition = $"M:({tradingCase.BuyPrice:0,0.00})";
                        }
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, M:{tradingCase.BuyPrice:0,0.00}, B:{tradingCase.SellPrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Mua {tradingCase.NumberStock:0,0} cổ giá {tradingCase.ActionPrice:0,0.00} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        tradingCase.AssetPosition = "100% T";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, M:{tradingCase.BuyPrice:0,0.00}, B:{tradingCase.SellPrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0}|-> Không giao dịch.");
                    }
                }
                else
                {
                    if (tradingCase.NumberChangeDay > 2)
                    {
                        tradingCase.IsSell = SellCondition(tradingHistory.Last().TradingDate, tradingHistory.Last().ClosePrice) > 0;
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
                            tradingCase.AddNote(tradingCase.ActionPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, M:{tradingCase.BuyPrice:0,0.00}, B:{tradingCase.SellPrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Bán {selNumberStock:0,0} cổ giá {tradingCase.ActionPrice:0,0.00} ({tradingCase.ActionPrice.GetPercent(lastBuyPrice):0,0.00}%), Max: ({tradingCase.MaxPriceOnBuy:0,0.00}) thuế {totalTax:0,0}");
                        }
                        else
                        {
                            tradingCase.AssetPosition = "100% C";
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, M:{tradingCase.BuyPrice:0,0.00}, B:{tradingCase.SellPrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AssetPosition = "100% C";
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, M:{tradingCase.BuyPrice:0,0.00}, B:{tradingCase.SellPrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản: {tradingCase.Profit(day.ClosePrice):0,0} |-> Không giao dịch do mới mua {tradingCase.NumberChangeDay} ngày");
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

            var sma10 = _sma_10.Find(chartPrice.TradingDate);
            if (sma10 is null || sma10.Sma is null)
            {
                return;
            }

            var sma6 = _sma_6.Find(chartPrice.TradingDate);
            if (sma6 is null || sma6.Sma is null)
            {
                return;
            }

            if (sma6.Sma < sma10.Sma && !tradingCase.ContinueBuy)
            {
                tradingCase.AddNote(0, $"{chartPrice.TradingDate:yy/MM/dd}: Cho phép lệnh mua được hoạt động do đường ma6 đã cắt xuống đường ma10.");
                tradingCase.ContinueBuy = true;
            }
        }

        public int BuyCondition(DateTime tradingDate, float lastClosePrice)
        {
            var rsi = _stochRsi_14_14_3_3.Find(tradingDate);
            if (rsi is null || rsi.StochRsi is null || rsi.Signal is null)
            {
                return 0;
            }

            if (rsi.StochRsi < 10 || rsi.StochRsi > 90)
            {
                return 0;
            }

            var sma36 = _sma_36.Find(tradingDate);
            if (sma36 is null || sma36.Sma is null)
            {
                return 0;
            }

            if (lastClosePrice < sma36.Sma)
            {
                return 0;
            }

            var sma10 = _sma_10.Find(tradingDate);
            if (sma10 is null || sma10.Sma is null)
            {
                return 0;
            }

            var sma6 = _sma_6.Find(tradingDate);
            if (sma6 is null || sma6.Sma is null)
            {
                return 0;
            }

            if (sma6.Sma < sma10.Sma)
            {
                return 0;
            }

            return 100;
        }

        public int SellCondition(DateTime tradingDate, float lastClosePrice)
        {
            if (lastClosePrice.GetPercent(tradingCase.ActionPrice) <= tradingCase.StopLossPercent)//Kiểm tra trạng thái bán chặn lỗ
            {
                tradingCase.AddNote(-1, $"{tradingDate:yy/MM/dd}: Kích hoạt lệnh bán chặn lỗ, giá mua {tradingCase.ActionPrice:0.0,00} giá kích hoạt {lastClosePrice:0.0,00}({lastClosePrice.GetPercent(tradingCase.ActionPrice):0.0,00})");
                tradingCase.ContinueBuy = false;
                return 100;
            }

            var sma10 = _sma_10.Find(tradingDate);
            if (sma10 is null || sma10.Sma is null)
            {
                return 0;
            }

            var sma6 = _sma_6.Find(tradingDate);
            if (sma6 is null || sma6.Sma is null)
            {
                return 0;
            }

            if (sma6.Sma > sma10.Sma)
            {
                return 0;
            }

            return 100;
        }

        public static float CalculateOptimalBuyPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.HighestPrice.GetPercent(q.OpenPrice)).Average() / 100;
            var buyPrice = rootPrice - (rootPrice * (percent * 10));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.OpenPrice.GetPercent(q.LowestPrice)).Average() / 100;
            var buyPrice = rootPrice + (rootPrice * (percent * 10));
            return (float)Math.Round((decimal)buyPrice, 2);
        }
    }
}