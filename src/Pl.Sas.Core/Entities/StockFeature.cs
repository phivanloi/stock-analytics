namespace Pl.Sas.Core.Entities
{
    public class StockFeature
    {
        /// <summary>
        /// Đường ema nhanh tối ưu nhất
        /// </summary>
        public int FastEma { get; set; } = 10;

        /// <summary>
        /// Đường ema chậm tối ưu nhất
        /// </summary>
        public int SlowEma { get; set; } = 28;

        /// <summary>
        /// Số lần chiến thắng bằng ema
        /// </summary>
        public int EmaWin { get; set; } = 0;

        /// <summary>
        /// Số lần thua bằng ema
        /// </summary>
        public int EmaLose { get; set; } = 0;
    }
}