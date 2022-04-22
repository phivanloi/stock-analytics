namespace Pl.Sas.Core.Entities
{
    public class BaseEntity
    {
        /// <summary>
        /// Khóa chính của entity
        /// vd 
        /// </summary>
        public string Id { get; set; } = Utilities.GenerateShortGuid();

        /// <summary>
        /// Thời điểm tạo mới entity
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Thời điểm cập nhập entity lần cuối cùng
        /// </summary>
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}