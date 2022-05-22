using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface ICorporateActionData
    {
        /// <summary>
        /// Lấy danh sách lịch sử sự kiện
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="eventCodes">Mã sự kiện
        /// <para>AGME,AGMR,BALLOT,BCHA,BOME,EGME => Đại hội Đồng Cổ đông</para>
        /// <para>AIS,NLIS,RETU,SUSP,TS => Niêm yết</para>
        /// <para>DDALL,DDIND,DDINS,DDRP => GD nội bộ</para>
        /// <para>DIV,ISS => Trả cổ tức</para>
        /// <para>KQCT,KQQY,KQSB’ => Kết quả kinh doanh</para>
        /// <para>AMEN,LIQUI,MA,MOVE,OTHE => Sự kiện khác</para>
        /// ex 'DIV','ISS'
        /// </param>
        /// <param name="fromDate">Giới hạn ngày phát hành sự kiện</param>
        /// <param name="toDate">Giới hạn ngày phát hành sự kiện</param>
        /// <returns>List CorporateAction</returns>
        Task<List<CorporateAction>> FindAllAsync(string symbol, string[] eventCodes, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Lấy lịch sử sự kiện của công ty
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>IReadOnlyList CorporateAction</returns>
        Task<IReadOnlyList<CorporateAction>> GetCorporateActionsForCheckDownloadAsync(string symbol);

        /// <summary>
        /// Lấy danh sách hiển thị
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="eventCodes">Mã sự kiện
        /// <para>AGME,AGMR,BALLOT,BCHA,BOME,EGME => Đại hội Đồng Cổ đông</para>
        /// <para>AIS,NLIS,RETU,SUSP,TS => Niêm yết</para>
        /// <para>DDALL,DDIND,DDINS,DDRP => GD nội bộ</para>
        /// <para>DIV,ISS => Trả cổ tức</para>
        /// <para>KQCT,KQQY,KQSB’ => Kết quả kinh doanh</para>
        /// <para>AMEN,LIQUI,MA,MOVE,OTHE => Sự kiện khác</para>
        /// ex 'DIV','ISS'
        /// </param>
        /// <param name="exchange">Sàn chứng khoán</param>
        Task<List<CorporateAction>> FindAllForViewPageAsync(string symbol, string[] eventCodes, string exchange);

        /// <summary>
        /// Lấy tất cả ngày giao dịch không hưởng quyền trả cổ tức để thực hiện tải lại lịch sử giá cổ phiếu
        /// </summary>
        /// <returns>List CorporateAction</returns>
        Task<List<CorporateAction>> GetTradingByExrightDateAsync();

        /// <summary>
        /// Thêm một danh sách lịch sử sự kiện
        /// </summary>
        /// <param name="corporateActions">Danh sách lịch sự sự kiện cần thêm mới</param>
        /// <returns>Task</returns>
        Task BulkInserAsync(IEnumerable<CorporateAction> corporateActions);

        /// <summary>
        /// Xóa một lịch sử sự kiện công ty bằng id
        /// </summary>
        /// <param name="id">Id cần xóa</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string id);
    }
}