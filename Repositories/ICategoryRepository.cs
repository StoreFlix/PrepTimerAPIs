using PrepTimerAPIs.Models;

namespace PrepTimerAPIs.Repositories
{
    public interface ICategoryRepository
    {
         Task<List<Ptcategory>> GetCategoriesByCompanyAsync(int companyId);
         Task AddCategoryAsync(Ptcategory category, List<int> storeIds);
    }
}
