using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

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
        /// Hỗ trợ tính toán lấy màu khi hiển thị
        /// </summary>
        /// <param name="first">Số cần kiểm tra</param>
        /// <param name="second">Số lấy mốc kiểm tra</param>
        /// <returns>string</returns>
        public static string GetTextColorCss(this float first, float second = 0)
        {
            if (first > second)
            {
                return "t-s";
            }
            else if (first == second)
            {
                return "t-wn";
            }
            else
            {
                return "t-d";
            }
        }

        /// <summary>
        /// Hỗ trợ tính toán lấy màu khi hiển thị
        /// </summary>
        /// <param name="first">Số cần kiểm tra</param>
        /// <param name="second">Số lấy mốc kiểm tra</param>
        /// <returns>string</returns>
        public static string GetTextColorCss(this int first, int second = 0)
        {
            if (first > second)
            {
                return "t-s";
            }
            else if (first == second)
            {
                return "t-wn";
            }
            else
            {
                return "t-d";
            }
        }

        /// <summary>
        /// Hỗ trợ tính toán lấy màu khi hiển thị cho một dải chỉ số có chặn đầu và chặn cuối
        /// Nếu giá trị nhỏ hơn chỉ số bắt đầu thì danger lớn hơn chỉ số kết thúc thì success và trong khoảng thì warning
        /// </summary>
        /// <param name="value">Giá trị cần check</param>
        /// <param name="start">Chỉ số bắt đầu</param>
        /// <param name="end">Chỉ số kết thúc</param>
        /// <returns>string</returns>
        public static string GetRangeTextColorCss(this int value, int start = -1, int end = 1)
        {
            if (value < start)
            {
                return "t-d";
            }
            else if (value > end)
            {
                return "t-s";
            }
            return "t-w";
        }

        /// <summary>
        /// Hàm lấy màu chữ cho chỉ báo Stochastic
        /// </summary>
        /// <param name="value">Giá trị cần check</param>
        /// <param name="start">Giá trị nền mặc định 20</param>
        /// <param name="end">Giá trị đỉnh mặc định 80</param>
        /// <returns>string</returns>
        public static string GetStochasticColorCss(this float value, int start = 20, int end = 80)
        {
            if (value < start)
            {
                return "t-s";
            }
            else if (value > end)
            {
                return "text-warning";
            }
            return "t-i";
        }

        /// <summary>
        /// Hỗ trợ tính toán lấy màu khi hiển thị cho một dải chỉ số có chặn đầu và chặn cuối
        /// Nếu giá trị nhỏ hơn chỉ số bắt đầu thì danger lớn hơn chỉ số kết thúc thì success và trong khoảng thì warning
        /// </summary>
        /// <param name="value">Giá trị cần check</param>
        /// <param name="start">Chỉ số bắt đầu</param>
        /// <param name="end">Chỉ số kết thúc</param>
        /// <returns>string</returns>
        public static string GetRangeTextColorCss(this float value, float start = -1, float end = 1)
        {
            if (value < start)
            {
                return "t-d";
            }
            else if (value > end)
            {
                return "t-s";
            }
            return "";
        }

        /// <summary>
        /// Hiển thị số tiền ra view
        /// </summary>
        /// <param name="money">Số tiền cần hiển thị</param>
        /// <param name="rate">Tỉ lệ</param>
        /// <returns>string</returns>
        public static string ShowMoney(this float money, int rate = 1000)
        {
            return $"{money / rate:0,0}";
        }

        /// <summary>
        /// Hiển thị giá cổ phiếu ra view
        /// </summary>
        /// <param name="money">Số tiền cần hiển thị</param>
        /// <param name="rate">Tỉ lệ</param>
        /// <returns>string</returns>
        public static string ShowPrice(this float money, int rate = 1000)
        {
            return $"{money / rate:0.00}";
        }

        /// <summary>
        /// Hiển thị giá cổ phiếu ra view
        /// </summary>
        /// <param name="money">Số tiền cần hiển thị</param>
        /// <param name="rate">Tỉ lệ</param>
        /// <returns>string</returns>
        public static string ShowPriceZezoToNull(this float money, int rate = 1000)
        {
            if (money == 0)
            {
                return "";
            }
            return $"{money / rate:0.00}";
        }

        /// <summary>
        /// Hiển thị % ra view
        /// </summary>
        /// <param name="percent">% cần hiển thị</param>
        /// <param name="format">cách hiển thị</param>
        /// <returns>string</returns>
        public static string ShowPercent(this float percent, string format = "0.0")
        {
            return $"{percent.ToString(format)}";
        }

        /// <summary>
        /// Lấy css class cho loại thông báo phân tích
        /// </summary>
        /// <param name="analyticsMessageType">Loại thông báo phân tích</param>
        /// <returns>string</returns>
        public static string GetAnalyticsTypeColorCss(this int analyticsMessageType)
        {
            if (analyticsMessageType <= -2)
            {
                return "danger";
            }
            if (analyticsMessageType >= 2)
            {
                return "primary";
            }
            return analyticsMessageType switch
            {
                -1 => "warning",
                0 => "info",
                1 => "success",
                _ => "",
            };
        }

        /// <summary>
        /// Chuyển một stock price to chart price
        /// </summary>
        /// <param name="stockPrice">stock price</param>
        /// <returns>ChartPrice</returns>
        public static ChartPrice ToChartPrice(this StockPrice stockPrice)
        {
            var changePercent = 0f;
            if (stockPrice.ClosePrice != 0)
            {
                changePercent = (stockPrice.ClosePrice - stockPrice.ClosePriceAdjusted) / stockPrice.ClosePrice;
            }
            return new()
            {
                Symbol = stockPrice.Symbol,
                TradingDate = stockPrice.TradingDate,
                OpenPrice = (stockPrice.OpenPrice - (stockPrice.OpenPrice * changePercent)) / 1000,
                HighestPrice = (stockPrice.HighestPrice - (stockPrice.HighestPrice * changePercent)) / 1000,
                LowestPrice = (stockPrice.LowestPrice - (stockPrice.LowestPrice * changePercent)) / 1000,
                ClosePrice = stockPrice.ClosePriceAdjusted / 1000,
                TotalMatchVol = stockPrice.TotalMatchVol,
                UpdatedTime = stockPrice.UpdatedTime,
                CreatedTime = stockPrice.CreatedTime,
                Type = "s",
            };
        }

        /// <summary>
        /// Chuyển một stock price to quote để tín chỉ báo
        /// </summary>
        /// <param name="chartPrice">chart price</param>
        /// <returns>ChartPrice</returns>
        public static Quote ToQuote(this ChartPrice chartPrice)
        {
            return new()
            {
                Close = (decimal)chartPrice.ClosePrice,
                Open = (decimal)chartPrice.OpenPrice,
                High = (decimal)chartPrice.HighestPrice,
                Low = (decimal)chartPrice.LowestPrice,
                Volume = (decimal)chartPrice.TotalMatchVol,
                Date = chartPrice.TradingDate
            };
        }
    }
}