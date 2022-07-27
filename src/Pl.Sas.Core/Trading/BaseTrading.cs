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
        protected const int _timeStockCome = 2;

        /// <summary>
        /// Hàm lấy tỉ lệ biến động giá của các sàn chứng khoán
        /// </summary>
        /// <param name="exchangeName">Tên sàn</param>
        /// <returns></returns>
        public static float GetExchangeFluctuationsRate(string exchangeName)
        {
            return exchangeName switch
            {
                "HOSE" => 0.07f,
                "HNX" => 0.1f,
                "UPCOM" => 0.15f,
                _ => 7,
            };
        }

        /// <summary>
        /// Lấy tỉ lệ cắt lỗ theo sàn chứng khoán
        /// </summary>
        /// <param name="exchangeName">Tên sàn</param>
        /// <returns>float</returns>
        public static float GetStopLossPercentRate(string exchangeName)
        {
            return exchangeName switch
            {
                "HOSE" => 0.07f,
                "HNX" => 0.08f,
                "UPCOM" => 0.09f,
                _ => 0.07f,
            };
        }

        /// <summary>
        /// Build tập hợp các chỉ báo theo lịch sử chart
        /// </summary>
        /// <param name="chartPrices">Danh sách dữ liệu chart sắp sếp theo ngày trading lớn dần</param>
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

        /// <summary>
        /// Hàm bán chứng khoán
        /// </summary>
        /// <param name="stockCount">Số chứng khoán cần bán</param>
        /// <param name="stockPrice">Giá bán</param>
        /// <returns></returns>
        public static (float TotalProfit, float TotalTax) Sell(long stockCount, float stockPrice)
        {
            var totalMoney = stockCount * stockPrice;
            var totalTax = totalMoney * _sellTax;
            return (totalMoney - totalTax, totalTax);
        }

        /// <summary>
        /// Hàm mua chứng khoán
        /// </summary>
        /// <param name="totalMoney">Tổng số tiền mua</param>
        /// <param name="stockPrice">Giá mua</param>
        /// <param name="numberChangeDay">số ngày mua của phiên trước</param>
        /// <returns></returns>
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

        /// <summary>
        /// Lấy thời gian trading cho các login realtime
        /// </summary>
        /// <param name="exchangeName">Tên sàn</param>
        /// <param name="checkTime">Thời gian check</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
                if (hour < 9)
                {
                    return TimeTrading.NST;
                }
                if (hour >= 15)
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

        /// <summary>
        /// Hàm tính lãi suất ngân hàng
        /// </summary>
        /// <param name="totalMoney">Tổng số tiền gửi</param>
        /// <param name="numberYear">Số năm gửi</param>
        /// <param name="interestRrate">lãi suất gửi tiết kiệm</param>
        /// <returns>float</returns>
        public static float BankProfit(float totalMoney, int numberYear = 5, float interestRrate = 6.8f)
        {
            var balanceMoney = totalMoney;
            for (int i = 0; i < numberYear; i++)
            {
                var subTotal = (balanceMoney / 100) * interestRrate;
                balanceMoney += subTotal;
            }
            return balanceMoney;
        }
    }

    /// <summary>
    /// Định nghĩa các khung thời gian trading
    /// </summary>
    public enum TimeTrading
    {
        ATO, //At the Opening => giao dịch lúc mở cửa
        CTA, //Continuous auction => trong phiên giao dịch
        ATC, //At the Close => giao dịch lúc đóng cửa
        PUT, //Put through => Khớp lệnh thỏa thuận
        TMP, //Time to MP => thời gian khớp lệnh mp vì sàn upcom không có khớp lệnh đóng cửa
        DON, //Done => kết thúc phiên giao dịch
        NST, //Not start trading => chưa bắt đầu giao dịch
    }
}