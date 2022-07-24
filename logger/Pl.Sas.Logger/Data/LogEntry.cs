namespace Pl.Sas.Logger.Data
{
    public class LogEntry
    {
        /// <summary>
        /// Id log
        /// </summary>
        public string Id { get; set; } = Utilities.GenerateShortGuid();

        /// <summary>
        /// Ngày tạo in 
        /// </summary>
        public long CreatedTime { get; set; }

        public string TimeView
        {
            get
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(CreatedTime);
                return date.ToString("MM/dd HH:mm:ss");
            }
        }

        /// <summary>
        /// Tiêu đề log
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Lội dung log
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Địa chỉ máy chủ, app phát sinh log
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Loại log
        /// 0 thông tin
        /// 1 cảnh báo
        /// 2 lỗi
        /// </summary>
        public byte Type { get; set; }
    }
}
