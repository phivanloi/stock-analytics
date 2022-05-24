using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class BaseTrading
    {
        protected const float _buyTax = 0.25f / 100;
        protected const float _sellTax = 0.35f / 100;
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

                indicatorSet.Add(chartPrices[i].DatePath, addItem);
            }
            return indicatorSet;
        }
    }
}