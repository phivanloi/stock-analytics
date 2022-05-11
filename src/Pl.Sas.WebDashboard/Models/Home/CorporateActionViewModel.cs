namespace Pl.Sas.WebDashboard.Models
{
    public class CorporateActionViewModel
    {
        public string Id { get; set; }

        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Tiêu dề sự kiện
        /// </summary>
        public string EventTitle { get; set; }

        /// <summary>
        /// Mã sàn chứng khoán
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Mô tả sự kiện
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Mã sự kiện
        /// <para>AGME,AGMR,BALLOT,BCHA,BOME,EGME => Đại hội Đồng Cổ đông</para>
        /// <para>AIS,NLIS,RETU,SUSP,TS => Niêm yết</para>
        /// <para>DDALL,DDIND,DDINS,DDRP => GD nội bộ</para>
        /// <para>DIV,ISS => Trả cổ tức</para>
        /// <para>KQCT,KQQY,KQSB’ => Kết quả kinh doanh</para>
        /// <para>AMEN,LIQUI,MA,MOVE,OTHE => Sự kiện khác</para>
        /// </summary>
        public string EventCode { get; set; }

        /// <summary>
        /// Ngày giao dịch không hướng quyền GDKHQ
        /// </summary>
        public string ExrightDate { get; set; }

        /// <summary>
        /// Ngày chốt
        /// </summary>
        public string RecordDate { get; set; }

        /// <summary>
        /// Ngày thực hiện
        /// </summary>
        public string IssueDate { get; set; }

        /// <summary>
        /// Ngày phát hành, ngày công bố
        /// </summary>
        public string PublicDate { get; set; }
    }
}