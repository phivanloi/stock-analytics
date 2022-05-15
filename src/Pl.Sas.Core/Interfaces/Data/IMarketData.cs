using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của market
    /// </summary>
    public interface IMarketData
    {
        /// <summary>
        /// Ghi lại dữ liệu chart
        /// </summary>
        /// <param name="chartPrices">dữ liệu chart</param>
        /// <param name="symbol">mã</param>
        /// <param name="type">loại chart</param>
        /// <returns>bool</returns>
        Task<bool> SaveChartPriceAsync(List<ChartPrice> chartPrices, string symbol, string type = "D");

        /// <summary>
        /// Lấy lịch sử giá cho trading
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetForTradingAsync(string symbol, DateTime fromDate, DateTime? toDate = null);

        /// <summary>
        /// Lấy danh sách sự kiện
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="exchange">mã sàn giao dịch</param>
        /// <param name="eventCode">Danh sách mã sự kiện</param>
        /// <returns>List CorporateAction</returns>
        Task<List<CorporateAction>> GetCorporateActionsAsync(string? symbol = null, string? exchange = null, string[]? eventCode = null);

        /// <summary>
        /// Lấy danh sách lịch sử giá cổ phiểu
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="numberItem">số bản ghi cần lấy</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetForStockViewAsync(string symbol, int numberItem = 10000);

        /// <summary>
        /// Lấy toàn bộ lịch sử giao dịch theo mã chứng khoán, mới nhất ở trên đầu
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>List StockPrice</returns>
        Task<List<StockPrice>> CacheGetAllStockPricesAsync(string symbol);

        /// <summary>
        /// Lấy thông tin chứng khoán theo mã
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        Task<Stock?> CacheGetStockByCodeAsync(string symbol);

        /// <summary>
        /// Lấy thông tin công ty theo mã chứng khoán
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>Company</returns>
        Task<Company?> CacheGetCompanyByCodeAsync(string symbol);

        /// <summary>
        /// Lấy danh sách sàn chứng khoán
        /// </summary>
        /// <returns>List string</returns>
        Task<List<string>> CacheGetExchangesAsync();

        /// <summary>
        /// Lấy danh sách cổ phiếu theo loại
        /// </summary>
        /// <param name="type">Loại</param>
        /// <returns>Stock</returns>
        Task<List<Stock>> GetStockByType(string type);

        /// <summary>
        /// Lấy danh sách lịch sử giá của chỉ số
        /// </summary>
        /// <param name="index">chỉ số</param>
        /// <param name="top">Số bản ghi cần lấy</param>
        /// <returns>List StockPrice</returns>
        Task<List<StockPrice>> GetAnalyticsTopIndexPriceAsync(string index, int top);

        /// <summary>
        /// Xóa lịch sử giá theo mã chứng khoán
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>bool</returns>
        Task<bool> DeleteStockPrices(string symbol);

        /// <summary>
        /// Lấy danh sách sự kiện vào ngày hôm nay để xử lý update lại giá mới
        /// </summary>
        /// <returns>List CorporateAction</returns>
        Task<List<CorporateAction>> GetCorporateActionTradingByExrightDateAsync();

        /// <summary>
        /// Lấy danh sách ngành
        /// </summary>
        /// <returns>List Industry</returns>
        Task<List<Industry>> GetIndustriesAsync();

        /// <summary>
        /// Lấy danh sách bao cáo khuyên nghị từ 6 tháng trở lên
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="top">Số báo cáo cần lấy</param>
        /// <returns>List StockRecommendation</returns>
        Task<List<StockRecommendation>> GetTopStockRecommendationInSixMonthAsync(string symbol, int top);

        /// <summary>
        /// Lấy top lịch sử giá cổ phiếu dùng để phân tích do đã chuyển đổi giá lớn nhât, giá nhỏ nhất
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="top">Số bản ghi cần lấy</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetAnalyticsTopStockPriceAsync(string symbol, int top);

        /// <summary>
        /// Lấy thông tin chứng khoán
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>Stock</returns>
        Task<Stock?> GetStockByCode(string symbol);

        /// <summary>
        /// Lấy top lịch sử giá cổ phiếu, sắp sếp ngày giao dịch giảm dần
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="top">Số bản ghi cần lấy</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetTopStockPriceAsync(string symbol, int top);

        /// <summary>
        /// Lấy thông tin tải sản và tỉ lệ cổ tức mới nhất theo mã cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cố phiếu</param>
        /// <returns>FinancialGrowth?</returns>
        Task<FinancialGrowth?> GetLastFinancialGrowthAsync(string symbol);

        /// <summary>
        /// Lấy thông tin giao dịch ngày cuối cùng
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>StockPrice</returns>
        Task<StockPrice?> GetLastStockPriceAsync(string symbol);

        /// <summary>
        /// Lấy thông tin đánh giá cổ phiểu của vnd
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>VndStockScore</returns>
        Task<List<VndStockScore>> GetVndStockScoreAsync(string symbol);

        /// <summary>
        /// Lấy thông tin tài chính
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>FiinEvaluated</returns>
        Task<FiinEvaluated?> GetFiinEvaluatedAsync(string symbol);

        /// <summary>
        /// Lấy báo cáo tài chính 5 năm các công ty cùng một ngành
        /// </summary>
        /// <param name="industryCode">Mã ngành</param>
        /// <param name="companies">Danh sách các công ty cần lấy báo cáo tài chính</param>
        /// <param name="yearRanger">Độ dài báo cáo khoảng 5 năm</param>
        /// <returns>List FinancialIndicatorReports</returns>
        Task<List<FinancialIndicator>> CacheGetFinancialIndicatorByIndustriesAsync(string industryCode, List<Company> companies, int yearRanger = 5);

        /// <summary>
        /// Lấy toàn bộ công ty trong hệ thống
        /// </summary>
        /// <param name="industryCode">mã ngành</param>
        /// <returns>List Company</returns>
        Task<List<Company>> CacheGetCompaniesAsync(string? industryCode = null);

        /// <summary>
        /// Lấy thông tin công ty theo mã chứng khoán
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        Task<Company?> GetCompanyAsync(string symbol);

        /// <summary>
        /// Ghi lại lịch sử giao dịch chứng khoán
        /// </summary>
        /// <param name="stockTransaction">Thông tin giao dịch</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockTransactionAsync(StockTransaction stockTransaction);

        /// <summary>
        /// Xử lý ghi lại điểm đánh giá chứng khoán của vnd
        /// </summary>
        /// <param name="vndStockScores">Thông tin đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveVndStockScoresAsync(List<VndStockScore> vndStockScores);

        /// <summary>
        /// Ghi lại đánh giá của vnd
        /// </summary>
        /// <param name="stockRecommendation">Thôn tin báo cáo cần ghi lại</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockRecommendationAsync(List<StockRecommendation> stockRecommendations);

        /// <summary>
        /// Ghi lại kết quả đánh giá 
        /// </summary>
        /// <param name="fiinEvaluate"></param>
        /// <returns></returns>
        Task<bool> SaveFiinEvaluateAsync(FiinEvaluated fiinEvaluate);

        /// <summary>
        /// Ghi lại danh sách cổ phiếu
        /// </summary>
        /// <param name="insertItems">Danh sách thêm mới</param>
        /// <param name="updateItems">Danh sách update</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockPriceAsync(List<StockPrice> insertItems, List<StockPrice> updateItems);

        /// <summary>
        /// Lấy lịch sử giá cổ phiếu bằng mã chứng khoán và ngày giao dịch
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="tradingDate">Ngày giao dịch</param>
        /// <returns>StockPrice</returns>
        Task<StockPrice?> GetStockPriceAsync(string symbol, DateTime tradingDate);

        /// <summary>
        /// Thêm mới hoạt động của công ty
        /// </summary>
        /// <param name="insertItems">Danh sách dữ liệu thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertCorporateActionAsync(List<CorporateAction> insertItems);

        /// <summary>
        /// Lấy danh sách hoạt động của công ty cho xử download
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>List CorporateAction</returns>
        Task<List<CorporateAction>> GetCorporateActionsForCheckDownloadAsync(string symbol);

        /// <summary>
        /// Hàm tạo và update các cổ phiếu
        /// </summary>
        /// <param name="insertItems">Danh sách các cổ phiếu cần tạo</param>
        /// <param name="updateItems">Danh sách các cổ phiếu cần update</param>
        /// <returns>bool</returns>
        Task<bool> InitialStockAsync(List<Stock> insertItems, List<Stock> updateItems);

        /// <summary>
        /// Lấy danh sách toàn bộ mã trong hệ thống đang có và chuyển sang dictionary symbol, obj
        /// </summary>
        /// <returns>Dictionary<string, Stock></returns>
        Task<Dictionary<string, Stock>> GetStockDictionaryAsync();

        /// <summary>
        /// Thêm mới hoặc sửa thông tin ngành
        /// </summary>
        /// <param name="industry">Thông tin ngành</param>
        /// <returns>bool</returns>
        Task<bool> SaveIndustryAsync(Industry industry);

        /// <summary>
        /// Ghi lại thông tin công ty
        /// </summary>
        /// <param name="company">Thông tin cần ghi</param>
        /// <returns>bool</returns>
        Task<bool> SaveCompanyAsync(Company company);

        /// <summary>
        /// Lấy danh sách lãnh đạo theo mã chứng khoán
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>List Leadership</returns>
        Task<List<Leadership>> GetLeadershipsAsync(string symbol);

        /// <summary>
        /// Ghi lại danh sách lãnh đạo của công ty
        /// </summary>
        /// <param name="insertItems">Danh sách cần thêm mới</param>
        /// <param name="deleteItems">Danh sách cần xóa</param>
        /// <returns>bool</returns>
        Task<bool> SaveLeadershipsAsync(List<Leadership> insertItems, List<Leadership> deleteItems);

        /// <summary>
        /// Lấy danh sách thông tin tăng trưởng tài chính của đoanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        Task<List<FinancialGrowth>> GetFinancialGrowthsAsync(string symbol);

        /// <summary>
        /// Ghi lại dữ liệu tăng trưởng tài chính
        /// </summary>
        /// <param name="insertItems">Danh sách thêm mới</param>
        /// <param name="updateItems">Danh sách update</param>
        /// <returns>bool</returns>
        Task<bool> SaveFinancialGrowthAsync(List<FinancialGrowth> insertItems, List<FinancialGrowth> updateItems);

        /// <summary>
        /// Lấy danh sách chỉ số tài chính của công ty
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>FinancialIndicator</returns>
        Task<List<FinancialIndicator>> GetFinancialIndicatorsAsync(string symbol);

        /// <summary>
        /// Ghi lại thông tin chỉ số tài chính
        /// </summary>
        /// <param name="insertItems">Danh sách thêm mới</param>
        /// <param name="updateItems">Danh sách update</param>
        /// <returns>bool</returns>
        Task<bool> SaveFinancialIndicatorAsync(List<FinancialIndicator> insertItems, List<FinancialIndicator> updateItems);
    }
}
