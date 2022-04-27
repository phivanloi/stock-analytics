using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của market
    /// </summary>
    public interface IMarketData
    {
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
        Task<StockPrice?> GeStockPriceAsync(string symbol, DateTime tradingDate);

        /// <summary>
        /// Update lại giá trị options của lịch
        /// </summary>
        /// <param name="schedule">Lịch cần update</param>
        /// <param name="key">Khóa</param>
        /// <param name="value">Giá trị</param>
        /// <returns></returns>
        Task<bool> UpdateKeyOptionScheduleAsync(Schedule schedule, string key, string value);

        /// <summary>
        /// Hàm update lịch
        /// </summary>
        /// <param name="schedule">Thông tin lịch cần update</param>
        /// <returns>bool</returns>
        Task<bool> UpdateScheduleAsync(Schedule schedule);

        /// <summary>
        /// Thêm mới hoạt động của công ty
        /// </summary>
        /// <param name="insertItems">Danh sách dữ liệu thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertCorporateActionAsync(List<CorporateAction> insertItems);

        /// <summary>
        /// Lấy danh sách hoạt động của công ty
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>List CorporateAction</returns>
        Task<List<CorporateAction>> GetCorporateActionsAsync(string symbol);

        /// <summary>
        /// Thêm mới một danh sách lịch
        /// </summary>
        /// <param name="schedules">Danh sách lịch cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertScheduleAsync(List<Schedule> schedules);

        /// <summary>
        /// Hàm tạo và update các cổ phiếu
        /// </summary>
        /// <param name="insertItems">Danh sách các cổ phiếu cần tạo</param>
        /// <param name="updateItems">Danh sách các cổ phiếu cần update</param>
        /// <returns>bool</returns>
        Task<bool> InitialStockAsync(List<Stock> insertItems, List<Stock> updateItems);

        /// <summary>
        /// Lấy thông tin đầy đủ của một Schedule
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>Schedule</returns>
        Task<Schedule?> GetScheduleByIdAsync(string id);

        /// <summary>
        /// Lấy thông tin đầy đủ của một Schedule trên cache
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>Schedule</returns>
        Task<Schedule?> CacheGetScheduleByIdAsync(string id);

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
