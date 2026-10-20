namespace PrepTimerAPIs.Dtos
{
    public class ItemDto
    {
        public int? ItemId { get; set; } // Nullable for Add
        public string ItemName { get; set; } = string.Empty;

        public int Duration { get; set; }
        public List<int>? Categories { get; set; }
        public List<TranslationDto>? Translations { get; set; }
        public int? CompanyId { get; set; }
        public bool IsDefault { get; set; }
        public bool CanEdit { get; set; }

        public string? IconName { get; set; }
        public string? IconUrl { get; set; }
        public IFormFile? icon { get; set; }
    }

    public class PTItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string IconName { get; set; }
        public string IconUrl { get; set; }
        public int Duration { get; set; }
        public bool IsDefault { get; set; }
        // True when the caller's company owns the item; shared items owned by Antunes can only be hidden
        public bool CanEdit { get; set; }
        public DateTime ModifiedOn { get; set; }

        public List<TranslationDto> Translations { get; set; }
        public List<int> Categories { get; set; }
    }

    public enum ItemDeleteResult
    {
        NotFound,
        Deleted,
        Hidden
    }

    public class TranslationDto
    {
        public string Locale { get; set; }
        public string ItemName { get; set; }
    }

}
