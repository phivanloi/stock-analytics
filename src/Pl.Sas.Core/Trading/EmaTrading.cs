using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class EmaTrading
    {
        public const float BuyTax = 0.25f / 100;
        public const float SellTax = 0.35f / 100;
        public const float AdvanceTax = 0.35f / 100;
        public const int batch = 100;
        public static readonly int[] Emas = new int[] { 5, 9, 12, 20, 36, 50, 100, 200 };

        public static (bool IsBuy, bool IsSell) Trading(TradingCase tradingCase, List<ChartPrice> chartPrices, Dictionary<string, IndicatorSet> indicatorSet)
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
                    continue;
                }

                var hasIndicator = indicatorSet.TryGetValue(previousChart.DatePath ?? "", out IndicatorSet? indicator);
                if (indicator is null)
                {
                    tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, không giao dịch do không có chỉ báo");
                    previousChart = day;
                    continue;
                }

                if (lastBuyPrice is null)
                {
                    var isBuy = BuyCondition(indicator);
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
                todayIsBuy = BuyCondition(lastIndicatorSet) > 0;
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

        public static int BuyCondition(IndicatorSet indicatorSet)
        {
            if (indicatorSet.Values["ema-100"] < indicatorSet.Values["ema-200"])
            {
                return 0;
            }
            if (indicatorSet.Values["ema-50"] < indicatorSet.Values["ema-100"])
            {
                return 0;
            }

            if (indicatorSet.Values["ema-12"] < indicatorSet.Values["ema-36"])
            {
                return 0;
            }

            if (indicatorSet.Values["ema-9"] < indicatorSet.Values["ema-20"])
            {
                return 0;
            }

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
            if (indicatorSet.Values["ema-12"] > indicatorSet.Values["ema-36"])
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

        public static Dictionary<string, IndicatorSet> BuildIndicatorSet(List<ChartPrice> chartPrices)
        {
            var indicatorSet = new Dictionary<string, IndicatorSet>();
            for (int i = 0; i < chartPrices.Count; i++)
            {
                var addItem = new IndicatorSet()
                {
                    TradingDate = chartPrices[i].TradingDate,
                    Values = new Dictionary<string, float>(),
                    ClosePrice = chartPrices[i].ClosePrice,
                };

                foreach (var ema in Emas)
                {
                    if ((i + 1) == ema)
                    {
                        addItem.Values.Add($"ema-{ema}", chartPrices.Skip(i + 1 - ema).Take(ema).Average(q => q.ClosePrice));
                    }
                    else if ((i + 1) > ema)
                    {
                        var yesterdaySet = indicatorSet[chartPrices[i - 1].DatePath];
                        var emaValue = ExponentialMovingAverage.ExponentialMovingAverageFormula(chartPrices[i].ClosePrice, yesterdaySet.Values[$"ema-{ema}"], ema);
                        addItem.Values.Add($"ema-{ema}", emaValue);
                    }
                }
                indicatorSet.Add(chartPrices[i].DatePath, addItem);
            }
            return indicatorSet;
        }
    }
}