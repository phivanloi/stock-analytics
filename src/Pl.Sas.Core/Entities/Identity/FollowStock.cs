namespace Pl.Sas.Core.Entities.Identity
{
    public class FollowStock : BaseEntity
    {
        /// <summary>
        /// Id người dùng 
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Mã chứng khoán quan tâm
        /// </summary>
        public string Symbol { get; set; } = null!;
    }
}