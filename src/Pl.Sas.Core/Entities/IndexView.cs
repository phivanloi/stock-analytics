namespace Pl.Sas.Core.Entities
{
    public class IndexView
    {
        #region Chỉ số DJI
        public string Dji { get; set; } = null!;
        public string DjiCss { get; set; } = null!;
        #endregion

        #region Chỉ số NASDAQ
        public string Nasdaq { get; set; } = null!;
        public string NasdaqCss { get; set; } = null!;
        #endregion

        #region Chỉ số SP500
        public string Sp500 { get; set; } = null!;
        public string Sp500Css { get; set; } = null!;
        #endregion

        #region Chỉ số FTSE100
        public string Ftse100 { get; set; } = null!;
        public string Ftse100Css { get; set; } = null!;
        #endregion

        #region Chỉ số CAC40
        public string Cac40 { get; set; } = null!;
        public string Cac40Css { get; set; } = null!;
        #endregion

        #region Chỉ số DAX
        public string Dax { get; set; } = null!;
        public string DaxCss { get; set; } = null!;
        #endregion

        #region Chỉ số KOSPI
        public string Kospi { get; set; } = null!;
        public string KospiCss { get; set; } = null!;
        #endregion

        #region Chỉ số N225
        public string N225 { get; set; } = null!;
        public string N225Css { get; set; } = null!;
        #endregion

        #region Chỉ số SHANGHAI
        public string Shanghai { get; set; } = null!;
        public string ShanghaiCss { get; set; } = null!;
        #endregion

        #region Chỉ số HANGSENG
        public string Hangseng { get; set; } = null!;
        public string HangsengCss { get; set; } = null!;
        #endregion

        #region Chỉ số VNINDEX
        public string Vnindex { get; set; } = null!;
        public string VnindexCss { get; set; } = null!;
        #endregion

        #region Chỉ số VNINDEX Value
        public string VnindexValue { get; set; } = null!;
        #endregion

        #region Kháng cự của vnindex
        public string KcVnindex { get; set; } = null!;
        #endregion

        #region Hỗ trợ của vnindex
        public string HtVnindex { get; set; } = null!;
        #endregion

        #region Chỉ số VNINDEXPe
        public string VnindexPe { get; set; } = null!;
        #endregion

        #region Chỉ số VNINDEXPb
        public string VnindexPb { get; set; } = null!;
        #endregion
    }
}