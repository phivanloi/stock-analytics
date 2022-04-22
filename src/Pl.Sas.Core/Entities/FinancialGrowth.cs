namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Tăng trưởng tài chính
    /// </summary>
    public class FinancialGrowth : BaseEntity
    {
        /// <summary>
        /// Mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// năm tăng trưởng
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tải sản
        /// </summary>
        public decimal Asset { get; set; }

        /// <summary>
        /// Cổ tức bằng tiền
        /// </summary>
        public decimal ValuePershare { get; set; }

        /// <summary>
        /// Vốn chủ sở hữu
        /// </summary>
        public decimal OwnerCapital { get; set; }
    }
}