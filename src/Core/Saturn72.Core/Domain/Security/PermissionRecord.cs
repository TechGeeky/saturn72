﻿namespace Saturn72.Core.Domain.Security
{
    public class PermissionRecord : DomainModelBase
    {
        /// <summary>
        ///     Gets or sets the permission name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the permission system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        ///     Gets or sets the permission category
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Gets or sets unique key for the permission
        /// </summary>
        public string UniqueKey { get; set; }
    }
}