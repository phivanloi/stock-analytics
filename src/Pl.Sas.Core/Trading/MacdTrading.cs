using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class MacdTrading
    {
        private const float _buyTax = 0.15f / 100;
        private const float SellTax = 0.25f / 100;
        private const float AdvanceTax = 0.15f / 100;
        private static List<MacdResult> _macd_12_26_9 = new();

        public static (bool IsBuy, bool IsSell) Trading(TradingCase tradingCase, List<ChartPrice> chartPrices)
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
                    tradingHistory.Add(day);
                    continue;
                }

                if (lastBuyPrice is null)
                {
                    var isBuy = BuyCondition(day.TradingDate);
                    if (isBuy > 0)
                    {
                        var optimalBuyPrice = CalculateOptimalBuyPrice(previousChart.OpenPrice, previousChart.LowestPrice, day.OpenPrice);
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
                        tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Mua {tradingCase.NumberStock:0,0} cổ giá {optimalBuyPrice * 1000:0,0} thuế {totalTax:0,0}");
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
                        var isSell = SellCondition(day.TradingDate);
                        if (isSell > 0)
                        {
                            var optimalSellPrice = CalculateOptimalSellPrice(previousChart.OpenPrice, previousChart.HighestPrice, day.OpenPrice);
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
                            tradingCase.AddNote(optimalSellPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{tradingCase.NumberStock:0,0}, Tải sản:{tradingCase.Profit(day.ClosePrice):0,0}, Bán {selNumberStock:0,0} cổ giá {optimalSellPrice * 1000:0,0} thuế {totalTax:0,0}");
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
            var lastHistory = chartPrices[^1];
            var todayIsBuy = false;
            var todayIsSell = false;
            if (lastBuyPrice != null)
            {
                todayIsSell = numberChangeDay > 2 && SellCondition(lastHistory.TradingDate) > 0;
            }
            else
            {
                todayIsBuy = BuyCondition(lastHistory.TradingDate) > 0;
            }

            return (todayIsBuy, todayIsSell);
        }

        public static (long StockCount, float ExcessCash, float TotalTax) Buy(float totalMoney, float stockPrice, int numberChangeDay)
        {
            if (stockPrice == 0)
            {
                return (0, totalMoney, 0);
            }

            var buyTax = _buyTax;
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

        public static float CalculateOptimalBuyPrice(float lastOpenPrice, float lastLowestPrice, float openPriceToday)
        {
            var buyPrice = openPriceToday - (openPriceToday / 100 * Math.Abs(lastLowestPrice.GetPercent(lastOpenPrice)));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static (float TotalProfit, float TotalTax) Sell(long stockCount, float stockPrice)
        {
            var totalMoney = stockCount * stockPrice;
            var totalTax = totalMoney * SellTax;
            return (totalMoney - totalTax, totalTax);
        }

        public static float CalculateOptimalSellPrice(float lastOpenPrice, float lastHighestPrice, float openPriceToday)
        {
            var sellPrice = openPriceToday + (openPriceToday / 100 * Math.Abs(lastOpenPrice.GetPercent(lastHighestPrice)));
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
            }).OrderBy(q => q.Date).ToList();
            _macd_12_26_9 = quotes.GetMacd(12, 36, 9, CandlePart.Close).ToList();
        }
    }
}