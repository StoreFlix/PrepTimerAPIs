using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFabricApp.API.Model
{
    public class PrepTimerResponse
    {

    }

    public class CondimentCategoriesResponse
    {
        /// <summary>
        /// CategoryId
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// CategoryName
        /// </summary>
        public string CategoryName { get; set; }

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
    }


    public class CondimentCategoryResponse
    {
        /// <summary>
        /// CategoryId
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// categoryLanguagesResponses
        /// </summary>
        public List<CategoryLanguages> categoryLanguages { get; set; }

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
    }


    public class CategoryLanguages
    {
        /// <summary>
        /// LangId
        /// </summary>
        public int LangId { get; set; }

        /// <summary>
        /// LangName
        /// </summary>
        public string LangName { get; set; }

        /// <summary>
        /// CategoryName
        /// </summary>
        public string CategoryName { get; set; }
    }


    public class LanguagesResponse
    {
        /// <summary>
        /// LangId
        /// </summary>
        public int LangId { get; set; }

        /// <summary>
        /// LangName
        /// </summary>
        public string LangName { get; set; }

        /// <summary>
        /// Locale
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// FilePath
        /// </summary>
        public string FilePath { get; set; }

    }

    public class CondimentCategoriesMappingResponse
    {
        /// <summary>
        /// CondimentId
        /// </summary>
        public int CondimentId { get; set; }

        /// <summary>
        /// CondimentName
        /// </summary>
        public string CondimentName { get; set; }

        /// <summary>
        /// TimePeriod
        /// </summary>
        public int TimePeriod { get; set; }

        /// <summary>
        /// CategoryNames
        /// </summary>
        public string CategoryNames { get; set; }

        /// <summary>
        /// IsEssential
        /// </summary>
        public bool IsEssential { get; set; }

        /// <summary>
        /// Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// LineNumber
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool Status { get; set; }
    }


    /// <summary>
    /// CondimentCategoryMappingResponse
    /// </summary>
    public class CondimentCategoryMappingResponse
    {
        /// <summary>
        /// CondimentId
        /// </summary>
        public int CondimentId { get; set; }

        /// <summary>
        /// condimentLanguages
        /// </summary>
        public List<CondimentMappingLanguages> condimentLanguages { get; set; }

        /// <summary>
        /// condimentLanguages
        /// </summary>
        public List<CondimentMappingCategories> condimentCategories { get; set; }

        /// <summary>
        /// Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// TimePeriod
        /// </summary>
        public int TimePeriod { get; set; }

        /// <summary>
        /// LineNumber
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// IsEssential
        /// </summary>
        public bool IsEssential { get; set; }
    }

    public class CondimentMappingLanguages
    {
        /// <summary>
        /// LangId
        /// </summary>
        public int LangId { get; set; }

        /// <summary>
        /// LangName
        /// </summary>
        public string LangName { get; set; }

        /// <summary>
        /// CondimentName
        /// </summary>
        public string CondimentName { get; set; }
    }

    public class CondimentMappingCategories
    {
        /// <summary>
        /// LangId
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// LangName
        /// </summary>
        public string CategoryName { get; set; }
    }
}
