namespace Pl.Sps.Core.Entities
{
    public class Stock : BaseEntity
    {
        /// <summary>
        /// mã giao dịch
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Tên cổ phiếu
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tên đẩy đủ
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Sàn niêm yết
        /// HOSE,HNX,UPCOM
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Loại cổ phiếu
        /// <para>stock => cổ phiếu</para>
        /// <para>index => các quỹ và chỉ số</para>
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Mã quản lý bên ssi
        /// </summary>
        public string SsiStockNo { get; set; }

        /// <summary>
        /// Tên công ty phát hành
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Tên công ty phát hành bằng tiếng anh
        /// </summary>
        public string CompanyNameEn { get; set; }
    }
}