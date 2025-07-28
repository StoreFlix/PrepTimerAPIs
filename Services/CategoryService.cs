using Microsoft.EntityFrameworkCore;
using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Repositories;

namespace PrepTimerAPIs.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly StoreLynkDbProd01Context _context;
        public CategoryService( StoreLynkDbProd01Context context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync(int companyId)
        {
            var categories = await _context.Ptcategories.Where(a => a.CompanyId == null || a.CompanyId == companyId).ToListAsync();
            return categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                IsDefault = c.CompanyId == null,
                LastModifiedOn = c.CreatedOn

            }).ToList();
        }

        public async Task AddCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Ptcategory
            {
                CategoryName = dto.CategoryName,
                CompanyId = 1
            };

             await _context.Ptcategories.AddAsync(category);
            await _context.SaveChangesAsync();

            if (dto.StoreIds.Any())
            {
                var storeMappings = dto.StoreIds.Select(storeId => new PtcategoryStoreMap
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

        public async Task<List<Ptlanguage>> GetTranslations()
        {
            var translations = await _context.Ptlanguages.ToListAsync();
            return translations;
           
        }

    }

}
