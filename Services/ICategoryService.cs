using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;

namespace PrepTimerAPIs.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync(int companyId);
        Task AddCategoryAsync(CreateCategoryDto dto);

        Task<bool> UpdateCategoryAsync(Ptcategory category);
        Task<bool> DeleteCategoryAsync(int id);

        Task<List<Ptlanguage>> GetTranslations();
    }

}
