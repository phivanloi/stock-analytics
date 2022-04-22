using Pl.Sps.Core.Entities;
using System;
using System.Collections.Generic;

namespace Pl.Sas.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Thêm thông báo vào danh sách
        /// </summary>
        /// <param name="analyticsMessages">Danh sách thông báo</param>
        /// <param name="message">Nội dung thông báo cần thêm</param>
        /// <param name="type">
        /// Loại thông báo
        /// <para>-2 nguy hiểm</para>
        /// <para>-1 cảnh báo</para>
        /// <para>0 là thông tin</para>
        /// <para>1 là tốt</para>
        /// <para>2 là rất tốt</para>
        /// </param>
        /// <param name="guideLink">Đường dẫn hướng dẫn</param>
        public static void Add(this List<AnalyticsMessage> analyticsMessages, string message, int score, int type, string guideLink = null)
        {
            analyticsMessages.Add(new AnalyticsMessage(message, score, type, guideLink));
        }

        /// <summary>
        /// Lấy % một số so với một số kiểu decimal
        /// </summary>
        /// <param name="startNum">số cần lấy</param>
        /// <param name="endNum">số so sánh</param>
        /// <returns>decimal</returns>
        public static decimal GetPercent(this decimal startNum, decimal endNum)
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
        public static string ResultString(this TradingCase tradingCase)
        {
            return $"Lợi nhuận {tradingCase.ProfitPercent:0,0.00}%, thuế {tradingCase.TotalTax:0,0}";
        }
    }
}