namespace Pl.Sas.Core.Entities
{
    public class StockFeature
    {
        /// <summary>
        /// Đường sma nhanh tối ưu nhất
        /// </summary>
        public int FastSma { get; set; } = 10;

        /// <summary>
        /// Đường sma chậm tối ưu nhất
        /// </summary>
        public int SlowSma { get; set; } = 28;

        /// <summary>
        /// Số lần chiến thắng bằng sma
        /// </summary>
        public int SmaWin { get; set; } = 0;

        /// <summary>
        /// Số lần thua bằng sma
        /// </summary>
        public int SmaLose { get; set; } = 0;
    }
}