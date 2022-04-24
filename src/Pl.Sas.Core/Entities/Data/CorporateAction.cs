namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Sự kiện quyền
    /// </summary>
    public class CorporateAction : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Tên sự kiện
        /// </summary>
        public string EventName { get; set; } = null!;

        /// <summary>
        /// Ngày giao dịch không hướng quyền GDKHQ
        /// </summary>
        public DateTime ExrightDate { get; set; }

        /// <summary>
        /// Ngày chốt
        /// </summary>
        public DateTime RecordDate { get; set; }

        /// <summary>
        /// Ngày thực hiện
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Tiêu dề sự kiện
        /// </summary>
        public string? EventTitle { get; set; }

        /// <summary>
        /// Ngày phát hành, ngày công bố
        /// </summary>
        public DateTime PublicDate { get; set; }

        /// <summary>
        /// Mã sàn chứng khoán
        /// </summary>
        public string Exchange { get; set; } = null!;

        /// <summary>
        /// Danh sách mã sự kiện
        /// </summary>
        public string? EventListCode { get; set; }

        /// <summary>
        /// Giá trị
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Tỉ lệ
        /// </summary>
        public float Ratio { get; set; }

        /// <summary>
        /// Mô tả sự kiện
        /// </summary>
        public byte[]? ZipDescription { get; set; } = null;

        /// <summary>
        /// Mã sự kiện
        /// <para>AGME,AGMR,BALLOT,BCHA,BOME,EGME => Đại hội Đồng Cổ đông</para>
        /// <para>AIS,NLIS,RETU,SUSP,TS => Niêm yết</para>
        /// <para>DDALL,DDIND,DDINS,DDRP => GD nội bộ</para>
        /// <para>DIV,ISS => Trả cổ tức</para>
        /// <para>KQCT,KQQY,KQSB’ => Kết quả kinh doanh</para>
        /// <para>AMEN,LIQUI,MA,MOVE,OTHE => Sự kiện khác</para>
        /// </summary>
        public string EventCode { get; set; } = null!;
    }
}