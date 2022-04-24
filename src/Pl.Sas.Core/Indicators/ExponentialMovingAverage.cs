namespace Pl.Sas.Core.Indicators
{
    public static class ExponentialMovingAverage
    {
        /// <summary>
        /// Tính toàn chỉ số ema
        /// </summary>
        /// <param name="todaysPrice">Giá ngày hôm nay</param>
        /// <param name="yesterdaysEMA">Giá ema ngày hôm qua</param>
        /// <param name="numberOfDays">Số ngày cần tính</param>
        /// <returns></returns>
        public static float ExponentialMovingAverageFormula(float todaysPrice, float yesterdaysEMA, float numberOfDays)
        {
            float multiplier = (2 / (numberOfDays + 1));
            return (todaysPrice * multiplier) + (yesterdaysEMA * (1 - multiplier));
        }
    }
}