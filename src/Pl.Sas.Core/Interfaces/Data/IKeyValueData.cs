using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IKeyValueData
    {
        /// <summary>
        /// Ghi lại giá trị và trả về giá trị đó
        /// </summary>
        /// <typeparam name="T">Kiểu giá trị</typeparam>
        /// <param name="key">Khóa giá trị</param>
        /// <param name="value">Giá trị</param>
        /// <returns>T</returns>
        Task<T> SetAsync<T>(string key, T value);

        /// <summary>
        /// Lấy giá trị
        /// </summary>
        /// <param name="key">Khóa giá trị</param>
        /// <returns>KeyValue</returns>
        Task<KeyValue> GetAsync(string key);
    }
}