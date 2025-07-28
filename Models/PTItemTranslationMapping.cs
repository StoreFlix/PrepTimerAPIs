namespace PrepTimerAPIs.Models
{
    public class PTItemTranslationMapping
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string? Locale { get; set; }
        public string? ItemName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public PTItem Item { get; set; }
    }
}
