using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;

namespace PrepTimerAPIs.Services
{
    public interface IItemService
    {
        Task<List<PTItemDto>> GetItemsAsync(int companyId);
        Task AddItemAsync(ItemDto dto, IFormFile? ItemIcon);
        Task<bool> UpdateItemAsync(ItemDto dto, IFormFile? ItemIcon);
        Task<bool> DeleteItemAsync(int id);
        Task<bool> CreateTestItem(CreateItemRequestDTO request);

        Task<ItemDto> GetItemByIdAsync(int id);
    }
}
