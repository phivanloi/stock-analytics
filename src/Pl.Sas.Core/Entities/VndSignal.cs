namespace Pl.Sas.Core.Entities
{
    public class VndSignal : BaseEntity
    {
        /// <summary>
        /// Mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Chiến thuật: cipShort => ngắn hạn, cipLong => dài hạn
        /// </summary>
        public string Strategy { get; set; }

        /// <summary>
        /// Tiếng hiệu: NEUTRAL => trung tính, BUY => mua, SELL => bán
        /// </summary>
        public string Signal { get; set; }

        /// <summary>
        /// Chi tiết nội dung, Object VndSignalResponse
        /// </summary>
        public byte[] DetailsZip { get; set; } = null;
    }
}