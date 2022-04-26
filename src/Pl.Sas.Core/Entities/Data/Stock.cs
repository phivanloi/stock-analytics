namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Thông tin cổ phiếu
    /// </summary>
    public class Stock : BaseEntity
    {
        public Stock(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// mã giao dịch
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Tên cổ phiếu
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Tên đẩy đủ
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Sàn niêm yết
        /// HOSE,HNX,UPCOM
        /// </summary>
        public string Exchange { get; set; } = null!;

        /// <summary>
        /// Loại cổ phiếu
        /// <para>s => cổ phiếu</para>
        /// <para>i => chỉ số thị trường</para>
        /// </summary>
        public string Type { get; set; } = null!;
    }
}