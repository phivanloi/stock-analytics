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
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// năm tăng trưởng
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tải sản
        /// </summary>
        public float Asset { get; set; }

        /// <summary>
        /// Cổ tức bằng tiền
        /// </summary>
        public float ValuePershare { get; set; }

        /// <summary>
        /// Vốn chủ sở hữu
        /// </summary>
        public float OwnerCapital { get; set; }
    }
}