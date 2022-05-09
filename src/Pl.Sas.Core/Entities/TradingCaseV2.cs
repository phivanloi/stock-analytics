namespace Pl.Sas.Core.Entities
{
    public class TradingCaseV2
    {
        public TradingCaseV2(bool isNote, float fixedCapital = 100000000)
        {
            IsNote = isNote;
            FixedCapital = fixedCapital;
        }

        /// <summary>
        /// Vốn cố định ban đầu
        /// </summary>
        public float FixedCapital { get; set; }

        /// <summary>
        /// Kết quả tradding
        /// </summary>
        public float Profit { get; set; }

        /// <summary>
        /// Tổng thuế phí, giao dịch
        /// </summary>
        public float TotalTax { get; set; }

        /// <summary>
        /// Phần trăm lợi nhuận
        /// </summary>
        public float ProfitPercent => (Profit - FixedCapital) * 100 / FixedCapital;

        #region Buy
        //Đường ema trên của điều kiện mua
        public int ConditionOverEmaBuy { get; set; }

        //Đường ema dưới của điều kiện mua
        public int ConditionUnderEmaBuy { get; set; }

        //Đường ema trên của lệnh mua
        public int OverEmaBuy { get; set; }

        //Đường ema dưới lệnh mua
        public int UnderEmaBuy { get; set; }
        #endregion

        #region Sell
        //Đường ema trên của điều kiện mua
        public int ConditionOverEmaSell { get; set; }

        //Đường ema dưới của điều kiện mua
        public int ConditionUnderEmaSell { get; set; }

        //Đường ema trên của lệnh bán
        public int OverEmaSell { get; set; }

        //Đường ema dưới lệnh bán
        public int UnderEmaSell { get; set; }
        #endregion

        #region Note
        /// <summary>
        /// Ghi chú diễn giải đầu tư với key -1, thua, 0 hòa hoặc không giao dịch, 1 là thắng. Value là nội dung ghi chú
        /// </summary>
        public List<KeyValuePair<int, string>> ExplainNotes { get; private set; } = new();

        /// <summary>
        /// Trạng thái có cho phép note ghi chú hay không
        /// </summary>
        public bool IsNote { get; private set; }

        /// <summary>
        /// Thêm ghi chú
        /// </summary>
        /// <param name="key">Trạng thái win/lose</param>
        /// <param name="message">Nội dung ghi chú</param>
        public void AddNote(int key, string message)
        {
            if (IsNote)
            {
                ExplainNotes.Add(new KeyValuePair<int, string>(key, message));
            }
        } 
        #endregion
    }
}