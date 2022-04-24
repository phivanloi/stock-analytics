namespace Pl.Sas.Core.Interfaces
{
    public interface IHttpHelper
    {
        /// <summary>
        /// Delete yêu cầu và nhận về json object
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu</typeparam>
        /// <param name="url">url cần delete</param>
        /// <returns></returns>
        Task<T?> DeleteJsonAsync<T>(string url);

        /// <summary>
        /// Get dữ liệu json
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu</typeparam>
        /// <param name="url">url cần lấy</param>
        /// <returns>T</returns>
        Task<T?> GetJsonAsync<T>(string url);

        /// <summary>
        /// Post yêu cầu và nhận về json object
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu</typeparam>
        /// <param name="url">url cần lấy</param>
        /// <param name="stringContent">Nội dung cần gửi</param>
        /// <returns>T</returns>
        Task<T?> PostJsonAsync<T>(string url, StringContent stringContent);

        /// <summary>
        /// Put yêu cầu và nhận về json object
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu</typeparam>
        /// <param name="url">url cần lấy</param>
        /// <param name="stringContent">Nội dung cần gửi</param>
        /// <returns>T</returns>
        Task<T?> PutJsonAsync<T>(string url, StringContent stringContent);
    }
}