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
    }
}