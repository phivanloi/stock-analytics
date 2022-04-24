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
        public string? DataKey { get; set; } = null!;

        /// <summary>
        /// Mức độ ưu tiên phổ từ 1 đến 100
        /// </summary>
        public int Priority { get; set; }

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
        /// <para>0 Tìm kiếm và bổ sung mã chứng khoáng vào hệ thống bằng ssi api</para>
        /// <para>1 Bổ sung thông tin doanh nghiệp bằng ssi api</para>
        /// <para>2 Bổ sung thông tin lãnh đạo doanh nghiệp bằng ssi api</para>
        /// <para>3 Bổ sung thông tin vốn và cổ tức cho doanh nghiệp bằng ssi api</para>
        /// <para>4 Bổ sung thông tin tài chính của danh nghiệp bằng ssi api</para>
        /// <para>5 Lấy danh sách lịch sử giá cổ phiếu theo ssi api</para>
        /// <para>6 Lấy danh sách lịch sử sự kiện công ty theo ssi api</para>
        /// <para>7 Lấy lịch sử khớp lệnh của cổ phiếu</para>
        /// <para>8 Bổ sung đánh giá cổ phiếu của fiin</para>
        /// <para>9 Thu thập lịch sử các chỉ số chính của thị trường</para>
        /// <para>10 Bổ sung lãi xuất ngân hàng cao nhất</para>
        /// <para>11 Bổ sung các chỉ số tài chính của các chỉ số trên thị trường trong vòng 3 năm gần nhất</para>
        /// <para>12 Thu thập giá tài nguyên, nguyên vật liệu</para>
        /// <para>13 Thu thập dữ liệu chiến tranh</para>
        /// <para>14 Thu thập dữ liệu dịch bệnh</para>
        /// <para>15 Thu thập dữ liệu thiên tai</para>
        /// <para>16 Thu thập dữ liệu đánh giá thể chế, chính sách</para>
        /// <para>17 Thu thập dữ liệu tiếng hiệu kỹ thuật của vnd</para>
        /// <para>18 Thu thập khuyến nghị của các công ty chứng khoán.</para>
        /// <para>19 Thu thập đánh giá cổ phiếu của vndirect.</para>
        /// <para>20 tải lịch sử giá cổ phiếu từ biểu đồ chart.</para>
        ///
        /// <para>200 Đánh giá giá trị của doanh nghiệp</para>
        /// <para>201 Phân tích kỹ thuật</para>
        /// <para>202 Thực hiện trading thử nghiệm trên bộ dữ liệu cũ</para>
        /// <para>203 Phân tích xử lý giá dự phóng của các công ty chứng khoán.</para>
        /// <para>204 Xử lý và đánh giá điểm mã ngành.</para>
        /// <para>205 Xử lý ngày giao dịch không hưởng quyền thì update lại lịch sử giá cổ phiếu</para>
        /// 
        /// <para>207 Đánh giá tâm lý thị trường</para>
        /// <para>208 Phân tích các yếu tố vĩ mô tác động đến cổ phiếu</para>
        /// <para>209 Phân tích tăng trưởng doanh nghiệp</para>
        /// <para>210 Phân tích đánh giá của fiintrading</para>
        /// <para>211 Phân tích đáng giá của vnd</para>
        ///
        /// <para>100 Dự đoán giá cố phiếu cho mô hình ftt</para>
        /// <para>101 Dự đoán giá cố phiếu cho mô hình ssa</para>
        /// <para>102 Phân loại su thế giá.</para>
        ///
        /// <para>300 Xử lý hiển thị dữ liệu chứng khoán cho hiển thị.</para>
        /// <para>301 Xử lý thông báo cho người dùng.</para>
        /// 
        /// <para>400 Tìm kiếm các chỉ báo kỹ thuật phù hợp cho chứng khoán</para>
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
                0 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),//tìm kiếm và bổ sung mã chứng khoáng vào hệ thống bằng ssi api
                1 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),//bổ sung thông tin doanh nghiệp bằng ssi api
                2 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),//bổ sung thông tin lãnh đạo doanh nghiệp bằng ssi api
                3 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),//bổ sung thông tin vốn và cổ tức cho doanh nghiệp bằng ssi api
                4 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),//bổ sung thông tin tài chính của danh nghiệp bằng ssi api
                5 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 30)),//lấy danh sách lịch sử giá cổ phiếu theo ssi api
                6 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(random.Next(0, 60)),//lấy danh sách lịch sử sự kiện công ty theo ssi api
                7 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(31, 60)),//lấy lịch sử khớp lệnh cổ phiếu
                8 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Bổ sung đánh giá cổ phiếu của fiin
                9 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//lấy lịch các chỉ báo chính của thị trường chứng khoán việt
                10 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Lấy lãi suất ngân hàng lớn nhất
                11 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Bổ sung các chỉ số tài chính của các chỉ số trên thị trường trong vòng 3 năm gần nhất
                12 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập giá tài nguyên, nguyên vật liệu
                13 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập dữ liệu chiến tranh
                14 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập dữ liệu dịch bệnh
                15 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập dữ liệu thiên tai
                16 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập dữ liệu thể chế chính sách
                17 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập đánh giá kỹ thuật của vnd
                18 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập khuyến nghị của các công ty chứng khoán
                19 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(0, 60)),//Thu thập đánh giá cổ phiếu của vndirect
                20 => baseTime.Date.AddDays(1).AddHours(2).AddMinutes(random.Next(31, 60)),//Tải bổ sung lịch sử giá cổ phiếu từ biểu dồ của vndirect

                200 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(10),//đánh giá giá trị của doanh nghiệp
                201 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(20),//phân tích kỹ thuật
                202 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(20),//Thực hiện trading thử nghiệm
                203 => baseTime.Date.AddDays(1).AddHours(3).AddMinutes(40),//Tính toán giá dự phóng của cổ phiếu của các công ty chứng khoán
                204 => baseTime.Date.AddDays(1).AddHours(3).AddMinutes(1),//Đánh giá su thế ngành chú ý làm trước khi phân tích đánh giá vĩ mô.
                205 => baseTime.Date.AddDays(1).AddHours(1).AddMinutes(10),//Sử lý ngày giao dịch không hưởng quyền thì update lại lịch sử giá cổ phiếu

                207 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(0),//Đánh giá tâm lý thị trường, chú ý đầy là phân tích để lấy dữ liệu đầu vào lên cần được chạy sớm
                208 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(0),//Phân tích các yếu tố vĩ mô tác động đến giá cổ phiếu
                209 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(15),//Phân tích tăng trưởng doanh nghiệp
                210 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(16),//Phân tích đánh giá của fiintrading
                211 => baseTime.Date.AddDays(1).AddHours(4).AddMinutes(17),//Phân tích đáng giá của vnd

                100 => baseTime.Date.AddDays(1).AddHours(5).AddMinutes(45),//Dự đoán giá cố phiếu cho mô hình ftt
                101 => baseTime.Date.AddDays(1).AddHours(5).AddMinutes(50),//Dự đoán giá cố phiếu cho mô hình ssa
                102 => baseTime.Date.AddDays(1).AddHours(5).AddMinutes(55),//Phân loại su thế giá theo mô hình SdcaMaximumEntropy

                300 => baseTime.AddMinutes(120),//Xử lý hiển thị dữ liệu chứng khoán cho hiển thị.
                301 => baseTime.Date.AddDays(1).AddHours(8),//Xử lý thông báo cho người dùng.

                400 => baseTime.Date.AddDays(30).AddHours(9).AddMinutes(0),//Tìm kiếm các chỉ báo kỹ thuật phù hợp cho chứng khoán

                _ => baseTime.AddHours(1),
            };
        }
    }
}