using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Thêm ghi chú phân tích vào danh sách ghi chú đã có
        /// </summary>
        /// <param name="analyticsNotes">Danh sách ghi chú hiện tại</param>
        /// <param name="message">Nội dung thông báo cần thêm</param>
        /// <param name="type">
        /// Loại thông báo
        /// <para>-2 nguy hiểm</para>
        /// <para>-1 cảnh báo</para>
        /// <para>0 bình thường</para>
        /// <para>1 là tốt</para>
        /// <para>2 là rất tốt</para>
        /// </param>
        /// <param name="guideLink">Đường link tìm hiểu về phân tích này</param>
        /// <returns>score</returns>
        public static int Add(this List<AnalyticsNote> analyticsNotes, string message, int score, int type, string? guideLink = null)
        {
            analyticsNotes.Add(new AnalyticsNote(message, score, type, guideLink));
            return score;
        }

        /// <summary>
        /// Lấy % một số so với một số kiểu decimal
        /// </summary>
        /// <param name="startNum">số cần lấy</param>
        /// <param name="endNum">số so sánh</param>
        /// <returns>decimal</returns>
        public static float GetPercent(this float startNum, float endNum)
        {
            if (endNum == 0 && startNum == 0)
            {
                return 0;
            }
            else if (endNum == 0)
            {
                return -100;
            }
            else if (startNum == 0)
            {
                return 100;
            }
            else
            {
                return (startNum - endNum) / Math.Abs(endNum) * 100;
            }
        }

        /// <summary>
        /// Lấy mô tả kết quả dạng string
        /// </summary>
        /// <param name="tradingCase">Trường hợp đầu tư cần lấy</param>
        /// <returns>string</returns>
        public static string ResultString(this TradingCaseV3 tradingCase)
        {
            return $"Lợi nhuận {tradingCase.ProfitPercent:0,0.00}%, thuế {tradingCase.TotalTax:0,0}";
        }
    }
}