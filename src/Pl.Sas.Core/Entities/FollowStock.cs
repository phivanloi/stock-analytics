﻿namespace Pl.Sas.Core.Entities
{
    public class FollowStock : BaseEntity
    {
        /// <summary>
        /// Id người dùng
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Mã chứng khoán quan tâm
        /// </summary>
        public string Symbol { get; set; }
    }
}