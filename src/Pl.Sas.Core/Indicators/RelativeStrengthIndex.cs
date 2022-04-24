using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Indicators
{
    public static class RelativeStrengthIndex
    {
        /// <summary>
        /// Tính toán chỉ số sực mạnh tương đối rsi
        /// </summary>
        /// <param name="stockPriceAdjs">Danh sách lịch sử giá xắp sếp theo ngày giao dịch giảm dần</param>
        /// <returns></returns>
        public static float CalculateRsi(IList<StockPrice> stockPriceAdjs)
        {
            float sumGain = 0;
            float sumLoss = 0;
            for (int i = stockPriceAdjs.Count - 1; i > 0; i--)
            {
                var difference = stockPriceAdjs[i - 1].ClosePriceAdjusted - stockPriceAdjs[i].ClosePriceAdjusted;
                if (difference >= 0)
                {
                    sumGain += difference;
                }
                else
                {
                    sumLoss -= difference;
                }
            }

            if (sumGain == 0) return 0;
            if (Math.Abs(sumLoss) < 10e-20f) return 100;

            var relativeStrength = sumGain / sumLoss;

            return 100.0f - (100.0f / (1 + relativeStrength));
        }
    }
}