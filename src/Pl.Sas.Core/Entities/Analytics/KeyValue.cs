using System.Text.Json;

namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Bảng khóa và giá trị trong hệ thống
    /// </summary>
    public class KeyValue : BaseEntity
    {
        /// <summary>
        /// Khóa truy cập
        /// </summary>
        public string Key { get; set; } = null!;

        /// <summary>
        /// Giá trị
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// Lấy giá trị
        /// </summary>
        /// <typeparam name="T">Kiểu của giá trị</typeparam>
        /// <returns>T</returns>
        /// <exception cref="Exception">Value is not type of T</exception>
        public T GetValue<T>() => JsonSerializer.Deserialize<T>(Value) ?? throw new Exception("Value is not type " + nameof(T));
    }
}