namespace Pl.Sas.Core.Entities
{
    public class Leadership : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán <see cref="Stock"/>
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// vị trí
        /// </summary>
        public string PositionName { get; set; } = null!;

        /// <summary>
        /// cấp vị trí
        /// </summary>
        public string? PositionLevel { get; set; }
    }
}