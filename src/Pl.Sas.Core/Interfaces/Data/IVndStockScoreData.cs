using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IVndStockScoreData
    {
        /// <summary>
        /// Lấy danh sách đánh giá cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>List VndStockScore</returns>
        Task<List<VndStockScore>> FindAllAsync(string symbol);

        /// <summary>
        /// Lấy một đánh giá cổ phiếu dựa vào mã cổ phiếu và ngày đánh giá
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="criteriaCode">Mã loại đánh giá</param>
        /// <returns></returns>
        Task<VndStockScore> FindAsync(string symbol, string criteriaCode);

        /// <summary>
        /// Ghi lại một đánh giá điểm số cho cổ phiếu
        /// </summary>
        /// <param name="vndStockScore">Thông tin cần đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveVndStockScoreAsync(VndStockScore vndStockScore);
    }
}