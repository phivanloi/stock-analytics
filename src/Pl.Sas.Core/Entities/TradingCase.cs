namespace Pl.Sas.Core.Entities
{
    public class TradingCase
    {
        public TradingCase(float fixedCapital = 100000000)
        {
            FixedCapital = fixedCapital;
            TradingMoney = fixedCapital;
        }

        #region Capital
        /// <summary>
        /// Vốn cố định ban đầu 
        /// </summary>
        public float FixedCapital { get; set; }

        /// <summary>
        /// Số lần thắng
        /// </summary>
        public int WinNumber { get; set; }

        /// <summary>
        /// Số lần thua
        /// </summary>
        public int LoseNumber { get; set; }

        /// <summary>
        /// Tiền mặt
        /// </summary>
        public float TradingMoney { get; set; } = 0;

        /// <summary>
        /// Giá chặn lỗ
        /// </summary>
        public float StopLossPrice { get; set; }

        /// <summary>
        /// Số cổ phiếu
        /// </summary>
        public long NumberStock { get; set; } = 0;

        /// <summary>
        /// Kết quả tradding
        /// </summary>
        public float Profit(float closePrice) => (NumberStock * closePrice * 1000) + TradingMoney;

        /// <summary>
        /// Tổng thuế phí, giao dịch
        /// </summary>
        public float TotalTax { get; set; }

        /// <summary>
        /// Phần trăm lợi nhuận
        /// </summary>
        public float ProfitPercent(float closePrice) => (Profit(closePrice) - FixedCapital) * 100 / FixedCapital;
        #endregion

        #region Current stage
        /// <summary>
        /// Cho phép mua, biến này được dùng để chặn mua mới cổ phiếu khi đi vào các trường hợp đặt biệt phải bán khi các chỉ số mua vẫn hợp lệ 
        /// vd: chạm chặn lỗ nhưng chỉ số vẫn cho phép giữ.
        /// Biến này sẽ được chuyển trạng thái về true khi các điều kiện bán được kích hoạt
        /// </summary>
        public bool ContinueBuy { get; set; } = true;

        /// <summary>
        /// Giá lớn nhất đạt được kể từ khi mua cổ phiếu
        /// </summary>
        public float MaxPriceOnBuy { get; set; } = 0;

        /// <summary>
        /// Trạng thái mua hôm nay
        /// </summary>
        public bool IsBuy { get; set; }

        /// <summary>
        /// Giá mua nếu là trạng thái mua
        /// </summary>
        public float BuyPrice { get; set; }

        /// <summary>
        /// Trạng thái bán hôm nay
        /// </summary>
        public bool IsSell { get; set; }

        /// <summary>
        /// Giá bán nếu là trạng thái bán
        /// </summary>
        public float SellPrice { get; set; }

        /// <summary>
        /// vị thế tài sản
        /// </summary>
        public string AssetPosition { get; set; } = "100% tiền";

        /// <summary>
        /// Giá thực hiện mua/bán. Khi mua giá này sẽ được lưu cho đến khi lệnh bán được kích hoạt và sau khi bán thì giá này sẽ được giữ cho đến khi lệnh mua được kích hoạt
        /// </summary>
        public float ActionPrice { get; set; } = 0;

        /// <summary>
        /// Số ngày thay đổi trạng thái
        /// </summary>
        public int NumberChangeDay { get; set; } = 1;

        /// <summary>
        /// Số ngày giữ cổ phiếu
        /// </summary>
        public int NumberDayInStock { get; set; } = 0;

        /// <summary>
        /// Số ngày giữ tiền
        /// </summary>
        public int NumberDayInMoney { get; set; } = 0;

        /// <summary>
        /// Số lần khớp giá mong muốn
        /// </summary>
        public int NumberPriceNeed { get; set; } = 0;

        /// <summary>
        /// Số lần khớp giá với giá đóng cửa
        /// </summary>
        public int NumberPriceClose { get; set; } = 0;
        #endregion

        #region Note
        /// <summary>
        /// Ghi chú diễn giải đầu tư với key -1, thua, 0 hòa hoặc không giao dịch, 1 là thắng. Value là nội dung ghi chú
        /// </summary>
        public List<KeyValuePair<int, string>> ExplainNotes { get; set; } = new();

        /// <summary>
        /// Thêm ghi chú
        /// </summary>
        /// <param name="key">Trạng thái win/lose</param>
        /// <param name="message">Nội dung ghi chú</param>
        public void AddNote(int key, string message)
        {
            if (key == 1)
            {
                WinNumber++;
            }
            if (key == -1)
            {
                LoseNumber++;
            }
            ExplainNotes.Add(new KeyValuePair<int, string>(key, message));
        }
        #endregion
    }
}