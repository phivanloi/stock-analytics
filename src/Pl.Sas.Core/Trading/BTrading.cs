using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class BTrading
    {
        public const float BuyTax = 0.25f / 100;
        public const float SellTax = 0.35f / 100;
        public const float AdvanceTax = 0.35f / 100;
        public const int batch = 100;

        public static (bool IsBuy, bool IsSell) Trading(TradingCase tradingCase, List<ChartPrice> chartPrices, Dictionary<string, IndicatorSet> indicatorSet)
        {
            var numberChangeDay = 10;
            var tradingHistory = new List<ChartPrice>();
            var tradingIndicator = new List<IndicatorSet>();
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

                indicatorSet.TryGetValue(previousChart.DatePath, out IndicatorSet? indicator);
                if (indicator is null)
                {
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, không giao dịch do không có chỉ báo");
                    previousChart = day;
                    tradingHistory.Add(day);
                    continue;
                }
                else
                {
                    tradingIndicator.Add(indicator);
                }

                if (lastBuyPrice is null)
                {
                    var isBuy = BuyCondition(tradingIndicator);
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
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, ema-12:{indicator.Values["ema-12"]:0,0.00}, ema-36:{indicator.Values["ema-36"]:0,0.00}, sub:{indicator.S("ema-12", "ema-36"):0,0.00} chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Mua {tradingCase.NumberStock:0,0} cổ giá {optimalBuyPrice * 1000:0,0} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, ema-12:{indicator.Values["ema-12"]:0,0.00}, ema-36:{indicator.Values["ema-36"]:0,0.00}, sub:{indicator.S("ema-12", "ema-36"):0,0.00} chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không gia dịch.");
                    }
                }
                else
                {
                    if (numberChangeDay > 2)
                    {
                        var isSell = SellCondition(indicator);
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
                            tradingCase.AddNote(optimalSellPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, ema-12:{indicator.Values["ema-12"]:0,0.00}, ema-36:{indicator.Values["ema-36"]:0,0.00}, sub:{indicator.S("ema-12", "ema-36"):0,0.00} chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Bán {selNumberStock:0,0} cổ giá {optimalSellPrice * 1000:0,0} thuế {totalTax:0,0}");
                            lastBuyPrice = null;
                        }
                        else
                        {
                            tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, ema-12:{indicator.Values["ema-12"]:0,0.00}, ema-36:{indicator.Values["ema-36"]:0,0.00}, sub:{indicator.S("ema-12", "ema-36"):0,0.00} chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch");
                        }
                    }
                    else
                    {
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, ema-12:{indicator.Values["ema-12"]:0,0.00}, ema-36:{indicator.Values["ema-36"]:0,0.00}, sub:{indicator.S("ema-12", "ema-36"):0,0.00} chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Không giao dịch do mới mua {numberChangeDay} ngày");
                    }
                }

                numberChangeDay++;
                previousChart = day;
                tradingHistory.Add(day);
            }
            var lastHistory = chartPrices[^1];
            var lastIndicatorSet = indicatorSet[lastHistory.DatePath];
            var todayIsBuy = false;
            var todayIsSell = false;
            if (lastBuyPrice != null)
            {
                todayIsSell = numberChangeDay > 2 && SellCondition(lastIndicatorSet) > 0;
            }
            else
            {
                todayIsBuy = BuyCondition(tradingIndicator) > 0;
            }

            return (todayIsBuy, todayIsSell);
        }

        public static (long StockCount, float ExcessCash, float TotalTax) Buy(float totalMoney, float stockPrice, int numberChangeDay)
        {
            if (stockPrice == 0)
            {
                return (0, totalMoney, 0);
            }

            var buyTax = BuyTax;
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

        public static int BuyCondition(List<IndicatorSet> indicatorSets)
        {
            if (indicatorSets.Count < 3)
            {
                return 0;
            }

            var lastItem = indicatorSets.Last();
            if (lastItem.Values["ema-100"] < lastItem.Values["ema-200"])
            {
                return 0;
            }
            if (lastItem.Values["ema-50"] < lastItem.Values["ema-100"])
            {
                return 0;
            }

            if (lastItem.Values["ema-12"] < lastItem.Values["ema-36"])
            {
                return 0;
            }

            if (lastItem.Values["ema-5"] < lastItem.Values["ema-12"])
            {
                return 0;
            }

            var avg = indicatorSets.Skip(indicatorSets.Count - 3).Take(3).Average(q => q.Values["ema-5"]);
            if (avg > lastItem.Values["ema-5"])
            {
                return 0;
            }
            //if (indicatorSet.Values["ema-9"] < indicatorSet.Values["ema-12"])
            //{
            //    return 0;
            //}

            //if (indicatorSet.Values["ema-5"] < indicatorSet.Values["ema-9"])
            //{
            //    return 0;
            //}
            return 100;
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

        public static int SellCondition(IndicatorSet indicatorSet)
        {
            //if (indicatorSet.Values["ema-12"] > indicatorSet.Values["ema-20"])
            //{
            //    return 0;
            //}

            if (indicatorSet.Values["ema-5"] > indicatorSet.Values["ema-12"])
            {
                return 0;
            }

            return 100;
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
            var quotes = chartPrices.Select(q => new Quote()
            {
                Close = (decimal)q.ClosePrice,
                Open = (decimal)q.OpenPrice,
                High = (decimal)q.HighestPrice,
                Low = (decimal)q.LowestPrice,
                Volume = (decimal)q.TotalMatchVol,
                Date = q.TradingDate
            });
            var df = new Quote();
            IEnumerable<MacdResult> results = quotes.GetMacd(12, 26, 9);
        }
    }
}