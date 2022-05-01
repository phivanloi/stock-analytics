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
        /// Thêm mới nếu chưa có hoặc sửa một key, value nếu đã có
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string AddOrUpdateOptions(string key, string value)
        {
            var options = JsonSerializer.Deserialize<Dictionary<string, string>>(OptionsJson) ?? throw new Exception("null OptionsJson");
            if (options.ContainsKey("key"))
            {
                options[key] = value;
            }
            else
            {
                options.Add(key, value);
            }
            OptionsJson = JsonSerializer.Serialize(options);
            return value;
        }

        /// <summary>
        /// Danh sách cặp key và giá trị hỗ trợ trong quá trình xử lý lịch
        /// </summary>
        public Dictionary<string, string> Options => JsonSerializer.Deserialize<Dictionary<string, string>>(OptionsJson) ?? throw new Exception("null OptionsJson");

        /// <summary>
        /// Loại của lịch, từ 0 đến 99 là lịch download, từ 100 đến 199 là lịch phân tích, từ 200 đến 299 là lịch hệ thống, từ 300 đến 399 là lịch hiển thị
        /// <para>0 Tìm kiếm và bổ sung mã chứng khoáng vào hệ thống</para>
        /// <para>10 Tải và phân tích dữ liệu cho từng cổ phiếu</para>
        /// <para>11 Tải và đánh giá tâm lý thị trường, các chỉ số</para>
        /// <para>12 Phân tích dòng tiền theo ngành</para>
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
                10 => baseTime.AddHours(1).AddMinutes(random.Next(0, 60)),
                11 => baseTime.AddHours(5).AddMinutes(random.Next(0, 60)),
                12 => baseTime.Date.AddDays(1).AddHours(5).AddMinutes(random.Next(0, 60)),
                _ => baseTime.AddHours(1),
            };
        }
    }
}