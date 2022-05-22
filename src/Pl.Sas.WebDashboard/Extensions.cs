using System.Security.Claims;

namespace Pl.Sas.WebDashboard
{
    public static class Extensions
    {
        /// <summary>
        /// Lấy IP của máy client gửi yêu cầu đến máy chủ
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>Id của máy khách</returns>
        public static string GetCurrentIpAddress(this HttpContext httpContext)
        {
            return httpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Lấy đường dẫn hiện tại của request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="includeQueryString">Có kèm theo các query string không</param>
        /// <returns>Đường dẫn web</returns>
        public static string GetThisPageUrl(this HttpContext httpContext, bool includeQueryString = true)
        {
            if (null == httpContext)
            {
                return string.Empty;
            }

            if (includeQueryString)
            {
                return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
            }
            else
            {
                return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}";
            }
        }

        /// <summary>
        /// Lấy giá trị cookie bằng key
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="key">Key</param>
        /// <returns>Trả về dạng string</returns>
        public static string GetCookieByKey(this HttpContext httpContext, string key)
        {
            var cookie = httpContext?.Request.Cookies[key];
            if (cookie != null)
            {
                return cookie;
            }
            return string.Empty;
        }

        /// <summary>
        /// Sét giá trị vào cookies
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="key">Key</param>
        /// <param name="value">Giá trị</param>
        /// <param name="expiresTime">Thời gian hết hạn tính bằng giây</param>
        /// <param name="path">Đường dẫn</param>
        public static void SetCookieByKey(this HttpContext httpContext, string key, string value, long? expiresTime = null, string path = "")
        {
            if (httpContext != null)
            {
                CookieOptions option = new()
                {
                    HttpOnly = false
                };
                if (expiresTime.HasValue)
                {
                    option.Expires = DateTime.Now.AddSeconds(expiresTime.Value);
                }

                if (string.IsNullOrEmpty(path))
                {
                    option.Path = path;
                }

                httpContext.Response.Cookies.Delete(key);
                httpContext.Response.Cookies.Append(key, value, option);
            }
        }

        /// <summary>
        /// Lấy user id người dùng đã đăng nhập
        /// </summary>
        /// <param name="httpContext">Current http context</param>
        /// <returns>Nếu user chưa đăng nhập thì trả về 0</returns>
        public static string GetUserId(this HttpContext httpContext)
        {
            if (null == httpContext || null == httpContext.User)
            {
                return string.Empty;
            }

            return httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
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
                return "text-success";
            }
            else if (first == second)
            {
                return "text-warning";
            }
            else
            {
                return "text-danger";
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
                return "text-success";
            }
            else if (first == second)
            {
                return "text-warning";
            }
            else
            {
                return "text-danger";
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
                return "text-danger";
            }
            else if (value > end)
            {
                return "text-success";
            }
            return "text-info";
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
                return "text-success";
            }
            else if (value > end)
            {
                return "text-warning";
            }
            return "text-info";
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
                return "text-danger";
            }
            else if (value > end)
            {
                return "text-success";
            }
            return "text-info";
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
    }
}