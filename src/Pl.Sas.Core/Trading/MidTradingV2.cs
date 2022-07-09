﻿using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class MidTradingV2 : BaseTrading
    {
        private readonly List<SmaResult> _slowSmas;
        private readonly List<SmaResult> _fastSmas;
        private readonly List<SmaResult> _limitSmas;
        private readonly List<SmaResult> _slowIndexSmas;
        private readonly List<SmaResult> _fastIndexSmas;
        private readonly List<ParabolicSarResult> _indexReverseSignals;
        private TradingCase tradingCase = new();

        public MidTradingV2(List<ChartPrice> chartPrices, List<ChartPrice> indexChartPrices)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            var indexQuotes = indexChartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _fastSmas = quotes.Use(CandlePart.Close).GetSma(12).ToList();
            _slowSmas = quotes.Use(CandlePart.Close).GetSma(26).ToList();
            _limitSmas = quotes.Use(CandlePart.Close).GetSma(36).ToList();
            _fastIndexSmas = indexQuotes.Use(CandlePart.Close).GetSma(6).ToList();
            _slowIndexSmas = indexQuotes.Use(CandlePart.Close).GetSma(10).ToList();
            _indexReverseSignals = indexQuotes.GetParabolicSar(0.02, 0.2).ToList();
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
                        tradingCase.StopLossPrice = tradingCase.ActionPrice - (tradingCase.ActionPrice * 0.07f);
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
                    if (tradingCase.NumberChangeDay > _timeStockCome)
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

            var slowSma = _slowSmas.Find(chartPrice.TradingDate);
            if (slowSma is null || slowSma.Sma is null)
            {
                return;
            }

            var fastSma = _fastSmas.Find(chartPrice.TradingDate);
            if (fastSma is null || fastSma.Sma is null)
            {
                return;
            }

            if (fastSma.Sma < slowSma.Sma && !tradingCase.ContinueBuy)
            {
                tradingCase.AddNote(0, $"{chartPrice.TradingDate:yy/MM/dd}: Cho phép lệnh mua được hoạt động do đường ma6 đã cắt xuống đường ma10.");
                tradingCase.ContinueBuy = true;
            }
        }

        public int BuyCondition(DateTime tradingDate, float lastClosePrice)
        {
            var slowIndexSma = _slowIndexSmas.Find(tradingDate);
            if (slowIndexSma is null || slowIndexSma.Sma is null)
            {
                return 0;
            }

            var fastIndexSma = _fastIndexSmas.Find(tradingDate);
            if (fastIndexSma is null || fastIndexSma.Sma is null)
            {
                return 0;
            }

            if (fastIndexSma.Sma < slowIndexSma.Sma)
            {
                return 0;
            }

            var indexSarSignal = _indexReverseSignals.Find(tradingDate);
            if (indexSarSignal is null || indexSarSignal.Sar is null)
            {
                return 0;
            }

            if (indexSarSignal.Sar > fastIndexSma.Sma)
            {
                return 0;
            }

            var limitSma = _limitSmas.Find(tradingDate);
            if (limitSma is null || limitSma.Sma is null)
            {
                return 0;
            }

            if (lastClosePrice < limitSma.Sma)
            {
                return 0;
            }

            var slowSma = _slowSmas.Find(tradingDate);
            if (slowSma is null || slowSma.Sma is null)
            {
                return 0;
            }

            var fastSma = _fastSmas.Find(tradingDate);
            if (fastSma is null || fastSma.Sma is null)
            {
                return 0;
            }

            if (fastSma.Sma < slowSma.Sma)
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

            var slowSma = _slowSmas.Find(tradingDate);
            if (slowSma is null || slowSma.Sma is null)
            {
                return 0;
            }

            var fastSma = _fastSmas.Find(tradingDate);
            if (fastSma is null || fastSma.Sma is null)
            {
                return 0;
            }

            if (fastSma.Sma > slowSma.Sma)
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