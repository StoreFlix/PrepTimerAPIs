namespace PrepTimerAPIs.Models
{
    public class PTItem
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? IconName { get; set; }
        public string? IconUrl { get; set; }
        public int Duration { get; set; }
        public int? CompanyId { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public ICollection<PTItemTranslationMapping> Translations { get; set; } = new List<PTItemTranslationMapping>();
        public ICollection<PTItemCategoryMapping> Categories { get; set; } = new List<PTItemCategoryMapping>();
    }

}
