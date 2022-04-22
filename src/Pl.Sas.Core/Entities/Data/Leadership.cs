using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pl.Sps.Core.Entities
{
    public class Leadership : BaseEntity
    {
        /// <summary>
        /// mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// vị trí
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// cấp vị trí
        /// </summary>
        public string PositionLevel { get; set; }

        /// <summary>
        /// Xếp hạng quản lý
        /// Thang điểm 10
        /// </summary>
        public int ManagementRank { get; set; } = 5;

        /// <summary>
        /// Còn hoạt động hay không
        /// </summary>
        public bool Activated { get; set; } = true;
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