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
