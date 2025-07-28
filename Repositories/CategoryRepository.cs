using PrepTimerAPIs.Models;
using Microsoft.EntityFrameworkCore;

namespace PrepTimerAPIs.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly StoreLynkDbProd01Context _context;

        public CategoryRepository(StoreLynkDbProd01Context context)
        {
            _context = context;
        }

        public async Task<List<Ptcategory>> GetCategoriesByCompanyAsync(int companyId)
        {
            return await _context.Ptcategories.Where(a => a.CompanyId == null || a.CompanyId == companyId).ToListAsync();


        }

        public async Task AddCategoryAsync(Ptcategory category, List<int> storeIds)
        {
            await _context.Ptcategories.AddAsync(category);
            await _context.SaveChangesAsync();

            if (storeIds.Any())
            {
                var storeMappings = storeIds.Select(storeId => new PtcategoryStoreMap
                {
                    CategoryId = category.CategoryId,
                    StoreId = storeId
                });

                await _context.PtcategoryStoreMaps.AddRangeAsync(storeMappings);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateCategoryAsync(Ptcategory category)
        {
            var existing = await _context.Ptcategories.FindAsync(category.CategoryId);
            if (existing == null) return false;

            existing.CategoryName = category.CategoryName;
           

            // If using a separate table for Store mappings, update logic here

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Ptcategories.FindAsync(id);
            if (category == null) return false;

            _context.Ptcategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
