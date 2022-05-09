namespace Pl.Sas.Infrastructure.Identity
{
    public static class PermissionConstants
    {
        #region System

        /// <summary>
        /// Access to cms
        /// </summary>
        public const string CmsDashbroad = "st-cd";

        /// <summary>
        /// System manager
        /// </summary>
        public const string SystemManager = "st-sm";

        #endregion System

        /// <summary>
        /// Get administrator role in system
        /// </summary>
        public static readonly IEnumerable<Permission> AdministratorRoles = new List<Permission>() {
            new Permission(CmsDashbroad, "Truy cập cms"),
            new Permission(SystemManager, "Quản trị hệ thống")
        };
    }

    public class Permission
    {
        /// <summary>
        /// Create an instance of permission
        /// </summary>
        /// <param name="role">Role key</param>
        /// <param name="name">Resource name</param>
        /// <param name="permissions">Children permission</param>
        public Permission(string role, string name, IEnumerable<Permission> permissions = null)
        {
            Role = role;
            Name = name;
            Permissions = permissions ?? new List<Permission>();
        }

        /// <summary>
        /// Role key
        /// </summary>
        public string Role { get; private set; }

        /// <summary>
        /// Resource name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Children of thi permission
        /// </summary>
        public IEnumerable<Permission> Permissions { get; set; } = new List<Permission>();
    }
}