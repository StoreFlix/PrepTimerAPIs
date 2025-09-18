namespace PrepTimerAPIs.Dtos
{
    public class CreateItemRequestDTO
    {
        public string ItemName { get; set; } = string.Empty;
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
