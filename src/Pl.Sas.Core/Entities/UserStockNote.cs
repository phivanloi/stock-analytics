namespace Pl.Sas.Core.Entities
{
    public class UserStockNote : BaseEntity
    {
        /// <summary>
        /// Id người dùng
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }
    }
}