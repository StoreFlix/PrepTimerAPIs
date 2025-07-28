using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFabricApp.API.Model
{
    public class PrepTimerRequest
    {
    }


    #region Condiment Category
    /// <summary>
    /// AddUpdateCondimentCategoryRequest
    /// </summary>
    public class AddUpdateCondimentCategoryRequest
    {
        /// <summary>
        /// CategoryId
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// startTime
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// EndTime
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// CategoryNames
        /// </summary>
        public List<CategoryNames> CategoryNames { get; set; }
    }

    public class CategoryNames
    {
        /// <summary>
        /// LanguageId
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// CategoryName
        /// </summary>
        public string CategoryName { get; set; }
    }
    #endregion


    #region Condiment Language
    /// <summary>
    /// AddUpdateCondimentLanguagesRequest
    /// </summary>
    public class AddUpdateCondimentLanguagesRequest
    {
        /// <summary>
        /// LangId
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// LangName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Locale
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// FilePath
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// FilePath
        /// </summary>
        public IFormFile? LangFile { get; set; }
    }
    #endregion


    #region Condiment Category Mapping
    /// <summary>
    /// AddUpdateCondimentCategoryMappingRequest
    /// </summary>
    public class AddUpdateCondimentCategoryMappingRequest
    {
        /// <summary>
        /// CondimentId
        /// </summary>
        public int CondimentId { get; set; }

        /// <summary>
        /// CondimentNames
        /// </summary>
        public List<CondimentNames> CondimentNames { get; set; }


        /// <summary>
        /// CondimentCategories
        /// </summary>
        public List<CondimentCategories> CondimentCategories { get; set; }

        /// <summary>
        /// CondimentInfo
        /// </summary>
        public List<CondimentInfo> CondimentInfo { get; set; }
    }

    /// <summary>
    /// CondimentNames
    /// </summary>
    public class CondimentNames
    {
        /// <summary>
        /// LanguageId
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// CondimentName
        /// </summary>
        public string CondimentName { get; set; }
    }

    /// <summary>
    /// CondimentCategories
    /// </summary>
    public class CondimentCategories
    {
        /// <summary>
        /// CategoryId
        /// </summary>
        public int CategoryId { get; set; }
    }

    /// <summary>
    /// CondimentInfo
    /// </summary>
    public class CondimentInfo
    {
        /// <summary>
        /// IsEssential
        /// </summary>
        public bool IsEssential { get; set; }

        /// <summary>
        /// TimePeriod
        /// </summary>
        public int TimePeriod { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// LineNumber
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Icon
        /// </summary>
        public string Icon { get; set; }
    }
    #endregion


   
}
