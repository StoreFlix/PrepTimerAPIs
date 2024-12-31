using System;
using System.Collections.Generic;


namespace ServiceFabricApp.API.Model
{
    /// <summary>
    /// HeaderRequest
    /// </summary>
    public class HeaderRequest
    {
        /// <summary>
        /// CompanyId
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// IsSuperAdminsss
        /// </summary>
        public bool IsSuperAdmin { get; set; }
    }
}
