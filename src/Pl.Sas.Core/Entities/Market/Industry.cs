namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Ngành nghề
    /// </summary>
    public class Industry : BaseEntity
    {
        /// <summary>
        /// Tên loại hình công ty
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mã ngành bên ssi crawler về
        /// </summary>
        public string Code { get; set; } = null!;
    }
}