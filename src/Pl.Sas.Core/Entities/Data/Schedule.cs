using System.Text.Json;

namespace Pl.Sas.Core.Entities
{
    public class Schedule : BaseEntity
    {
        /// <summary>
        /// Tên lịch
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Lưu thông tin khóa dữ liệu cho lịch, có thể là mã chứng khoán hoặc có thể là id bản ghi vv
        /// </summary>
        public string? DataKey { get; set; }

        /// <summary>
        /// Trạng thái hoạt động hay không
        /// </summary>
        public bool Activated { get; set; } = true;

        /// <summary>
        /// Thời gian kịch hoạt lịch
        /// mỗi lần được kịch hoạt sẽ update lại thời gian cho lần chạy tiếp theo
        /// </summary>
        public DateTime ActiveTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Đánh dấu lịch này có bị lỗi hay không
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Một chuỗi json dic
        /// </summary>
        public string OptionsJson { get; set; } = JsonSerializer.Serialize(new Dictionary<string, string>());

        /// <summary>
        /// Danh sách cặp key và giá trị hỗ trợ trong quá trình xử lý lịch
        /// </summary>
        public Dictionary<string, string> Options => JsonSerializer.Deserialize<Dictionary<string, string>>(OptionsJson) ?? throw new Exception("null OptionsJson");

        /// <summary>
        /// Loại của lịch
        /// <para>0 Tìm kiếm và bổ sung mã chứng khoáng vào hệ thống</para>
        /// <para>1 Tải và phân tích dữ liệu cho từng cổ phiếu</para>
        /// <para>2 Đánh giá tâm lý thị trường</para>
        /// <para>3 Phân tích dòng tiền theo ngành</para>
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Áp dụng thời gian chạy lịch tiếp theo
        /// </summary>
        /// <param name="baseTime"></param>
        public void ApplyActiveTime(DateTime baseTime)
        {
            var random = new Random();
            ActiveTime = Type switch
            {
                0 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),
                1 => baseTime.AddHours(1).AddMinutes(random.Next(0, 60)),
                2 => baseTime.AddHours(5).AddMinutes(random.Next(0, 60)),
                3 => baseTime.Date.AddDays(1).AddHours(5).AddMinutes(random.Next(0, 60)),
                _ => baseTime.AddHours(1),
            };
        }
    }
}