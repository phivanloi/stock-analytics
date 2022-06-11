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

        #region Chỉ số DAX
        public string Dax { get; set; } = null!;
        public string DaxCss { get; set; } = null!;
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

        #region Chỉ số VNINDEXPe
        public string VnindexPe { get; set; } = null!;
        #endregion

        #region Chỉ số VNINDEXPb
        public string VnindexPb { get; set; } = null!;
        #endregion
    }
}