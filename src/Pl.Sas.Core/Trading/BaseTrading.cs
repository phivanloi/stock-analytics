using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class BaseTrading
    {
        protected const float _buyTax = 0.15f / 100;
        protected const float _sellTax = 0.25f / 100;
        protected const float _advanceTax = 0.15f / 100;
        protected const int _batch = 100;

        /// <summary>
        /// Hàm lấy tỉ lệ biến động giá của các sàn chứng khoán
        /// </summary>
        /// <param name="exchangeName">Tên sàn</param>
        /// <param name="isStartTrading">Có là ngày đầu tiên bắt đầu giao dịch hay không hoặc là ngày đầu tiên sau 25 phiên không giao dịch</param>
        /// <returns></returns>
        public static float GetExchangeFluctuationsRate(string exchangeName)
        {
            return exchangeName switch
            {
                "HOSE" => 7,
                "HNX" => 10,
                "UPCOM" => 15,
                _ => 7,
            };
        }

        /// <summary>
        /// Build tập hợp các chỉ báo theo lịch sử chart
        /// </summary>
        /// <param name="chartPrices">Danh sách dữ liệu chart sắp sếp theo ngày trang dinh lớn dần</param>
        /// <returns>Dictionary string, IndicatorSet</returns>
        public static Dictionary<string, IndicatorSet> BuildIndicatorSet(List<ChartPrice> chartPrices)
        {
            var indicatorSet = new Dictionary<string, IndicatorSet>();
            var maPeriod = new List<int>();
            for (int p = 1; p <= 50; p++)
            {
                maPeriod.Add(p);
            }
            maPeriod.AddRange(new List<int>() { 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200 });
            for (int i = 0; i < chartPrices.Count; i++)
            {
                var addItem = new IndicatorSet()
                {
                    TradingDate = chartPrices[i].TradingDate,
                    Values = new Dictionary<string, float>(),
                    ClosePrice = chartPrices[i].ClosePrice,
                };

                foreach (var maIndex in maPeriod)
                {
                    if ((i + 1) == maIndex)
                    {
                        addItem.Values.Add($"ema-{maIndex}", chartPrices.Skip(i + 1 - maIndex).Take(maIndex).Average(q => q.ClosePrice));
                    }
                    else if ((i + 1) > maIndex)
                    {
                        var yesterdaySet = indicatorSet[chartPrices[i - 1].DatePath];
                        var emaValue = ExponentialMovingAverage.ExponentialMovingAverageFormula(chartPrices[i].ClosePrice, yesterdaySet.Values[$"ema-{maIndex}"], maIndex);
                        addItem.Values.Add($"ema-{maIndex}", emaValue);
                    }
                }

                if (!indicatorSet.ContainsKey(chartPrices[i].DatePath))
                {
                    indicatorSet.Add(chartPrices[i].DatePath, addItem);
                }
            }
            return indicatorSet;
        }

        public static (float TotalProfit, float TotalTax) Sell(long stockCount, float stockPrice)
        {
            var totalMoney = stockCount * stockPrice;
            var totalTax = totalMoney * _sellTax;
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
                buyTax += _advanceTax;
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

        public static TimeTrading GetTimeTrading(string exchangeName, DateTime checkTime)
        {
            return exchangeName switch
            {
                "HOSE" => HoseTime(checkTime),
                "HNX" => HnxTime(checkTime),
                "UPCOM" => UpcomTime(checkTime),
                _ => throw new Exception("GetTimeTrading can't find exchange definition."),
            };

            TimeTrading UpcomTime(DateTime time)
            {
                var hour = time.Hour;
                var minute = time.Minute;
                if (hour < 9 || hour >= 15)
                {
                    return TimeTrading.DON;
                }

                if (hour == 14 && minute >= 55)
                {
                    return TimeTrading.TMP;
                }

                return TimeTrading.CTA;
            }

            TimeTrading HnxTime(DateTime time)
            {
                var hour = time.Hour;
                var minute = time.Minute;
                if (hour < 9)
                {
                    return TimeTrading.NST;
                }

                if (hour >= 15)
                {
                    return TimeTrading.DON;
                }

                if (hour == 14 && minute >= 30 && minute < 45)
                {
                    return TimeTrading.ATC;
                }

                if (hour == 14 && minute >= 45)
                {
                    return TimeTrading.PUT;
                }

                return TimeTrading.CTA;
            }

            TimeTrading HoseTime(DateTime time)
            {
                var hour = time.Hour;
                var minute = time.Minute;
                if (hour < 9)
                {
                    return TimeTrading.NST;
                }

                if (hour >= 15)
                {
                    return TimeTrading.DON;
                }

                if (hour == 9 && minute < 15)
                {
                    return TimeTrading.ATO;
                }

                if (hour == 14 && minute >= 30 && minute < 45)
                {
                    return TimeTrading.ATC;
                }

                if (hour == 14 && minute >= 45)
                {
                    return TimeTrading.PUT;
                }

                return TimeTrading.CTA;
            }
        }
    }

    /// <summary>
    /// Định nghĩa các khung thời gian trading
    /// </summary>
    public enum TimeTrading
    {
        ATO, //At the Opening
        CTA, //Continuous auction
        ATC, //At the Close
        PUT, //Put through
        TMP, //Time to MP
        DON, //Done
        NST, //Not start trading
    }
}