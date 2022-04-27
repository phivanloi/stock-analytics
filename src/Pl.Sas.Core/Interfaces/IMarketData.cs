﻿using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của market
    /// </summary>
    public interface IMarketData
    {
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
        /// <param name="symbol"></param>
        /// <returns></returns>
        Task<List<FinancialGrowth>> GetFinancialGrowthsAsync(string symbol);

        /// <summary>
        /// Ghi lại dữ liệu tăng trưởng tài chính
        /// </summary>
        /// <param name="insertItems">Danh sách thêm mới</param>
        /// <param name="updateItems">Danh sách update</param>
        /// <returns>bool</returns>
        Task<bool> SaveFinancialGrowthAsync(List<FinancialGrowth> insertItems, List<FinancialGrowth> updateItems);
    }
}
