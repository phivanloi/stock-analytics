﻿namespace Pl.Sas.Core.Entities
{
    public class TradingCase
    {
        public TradingCase(bool isNote = false, float fixedCapital = 100000000)
        {
            IsNote = isNote;
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
        /// Trạng thái tài sản
        /// </summary>
        public string AssetStats
        {
            get
            {
                if (NumberStock <= 0)
                {
                    return "Tiền mặt";
                }
                return "Chứng khoán";
            }
        }
        #endregion

        #region Note
        /// <summary>
        /// Ghi chú diễn giải đầu tư với key -1, thua, 0 hòa hoặc không giao dịch, 1 là thắng. Value là nội dung ghi chú
        /// </summary>
        public List<KeyValuePair<int, string>> ExplainNotes { get; set; } = new();

        /// <summary>
        /// Trạng thái có cho phép note ghi chú hay không
        /// </summary>
        public bool IsNote { get; set; }

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
            if (IsNote)
            {
                ExplainNotes.Add(new KeyValuePair<int, string>(key, message));
            }
        }
        #endregion
    }
}