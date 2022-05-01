using System.Diagnostics.CodeAnalysis;

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

    public class LeadershipComparer : IEqualityComparer<Leadership>
    {
        public bool Equals([AllowNull] Leadership l1, [AllowNull] Leadership l2)
        {
            if (l1 is null && l2 is null)
                return true;
            else if (l1 is null || l2 is null)
                return false;
            else if (l1.FullName == l2.FullName && l1.PositionName == l2.PositionName)
                return true;
            else
                return false;
        }

        public int GetHashCode([DisallowNull] Leadership obj)
        {
            return obj.GetHashCode();
        }
    }
}