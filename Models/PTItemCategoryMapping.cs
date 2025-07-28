namespace PrepTimerAPIs.Models
{
    public class PTItemCategoryMapping
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public PTItem Item { get; set; }
    }
}
