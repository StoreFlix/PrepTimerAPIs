namespace PrepTimerAPIs.Dtos
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public bool IsDefault { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}
