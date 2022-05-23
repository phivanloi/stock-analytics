using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IFiinEvaluatedData
    {
        /// <summary>
        /// Lấy danh sách đanh giá của fiintrading
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>List FiinEvaluate</returns>
        Task<IReadOnlyList<FiinEvaluated>> FindAllAsync(string symbol);

        /// <summary>
        /// Lấy chi tiết một đánh giá của fiintrading cho một mã cổ phiếu và một phiên giao dịch
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>FiinEvaluate</returns>
        Task<FiinEvaluated> FindAsync(string symbol);

        /// <summary>
        /// Ghi lại đánh giá của fiin, nếu chưa có sẽ insert và có rồi sẽ update
        /// </summary>
        /// <param name="fiinEvaluated">Thông tin chi tiết</param>
        /// <returns>bool</returns>
        Task<bool> SaveFiinEvaluateAsync(FiinEvaluated fiinEvaluated);
    }
}