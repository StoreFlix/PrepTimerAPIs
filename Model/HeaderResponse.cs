using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFabricApp.API.Model
{
    /// <summary>
    /// HeaderResponse
    /// </summary>
    public class HeaderResponse
    {
        /// <summary>
        /// UserLoginName
        /// </summary>
        public string UserLoginName { get; set; }

        /// <summary>
        /// UserFirstName
        /// </summary>
        public string UserFirstName { get; set; }

        /// <summary>
        /// UserLastName
        /// </summary>
        public string UserLastName { get; set; }

        /// <summary>
        /// Home
        /// </summary>
        public string Home { get; set; }

        /// <summary>
        /// CompanyLogo
        /// </summary>
        public string CompanyLogo { get; set; }
        /// <summary>
        /// ThemeName
        /// </summary>
        public string ThemeName { get; set; }

        /// <summary>
        /// IsSuperAdmin
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// HeaderMenuList
        /// </summary>
        public List<HeaderMenuModels> HeaderMenuList { get; set; }
    }


    /// <summary>
    /// HeaderMenuModels
    /// </summary>
    public class HeaderMenuModels
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// MenuName
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// IsMainMenu
        /// </summary>
        public bool IsMainMenu { get; set; }

        /// <summary>
        /// ParentId
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// MenuURL
        /// </summary>
        public string MenuURL { get; set; }

        /// <summary>
        /// DisplayName
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// MenuOrder
        /// </summary>
        public int? MenuOrder { get; set; }
                
    }
}
