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
        private static readonly TradingCase tradingCase = new();

        public static TradingCase Trading(List<ChartPrice> chartPrices, bool isNoteTrading = true)
        {
            tradingCase.IsNote = isNoteTrading;
            BuildIndicatorSet(chartPrices);
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
                tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, previousChart.ClosePrice);
                tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, previousChart.ClosePrice);

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
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(3).Select(q => Math.Abs(q.OpenPrice.GetPercent(q.HighestPrice))).Average() / 100;
            var buyPrice = openPriceToday - (openPriceToday * percent);
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float openPriceToday)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(3).Select(q => Math.Abs(q.OpenPrice.GetPercent(q.LowestPrice))).Average() / 100;
            var buyPrice = openPriceToday + (openPriceToday * percent);
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static (float TotalProfit, float TotalTax) Sell(long stockCount, float stockPrice)
        {
            var totalMoney = stockCount * stockPrice;
            var totalTax = totalMoney * SellTax;
            return (totalMoney - totalTax, totalTax);
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

        private static void BuildIndicatorSet(List<ChartPrice> chartPrices)
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
    }
}