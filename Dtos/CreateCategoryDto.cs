namespace PrepTimerAPIs.Dtos
{
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = null!;
        public List<int> StoreIds { get; set; } = new();
    }
}
