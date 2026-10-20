using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.Extensions.Configuration;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace PrepTimerAPIs.Services
{
    public class ItemService :IItemService
    {

        private readonly StoreLynkDbProd01Context _context;
        private readonly IConfiguration _configuration;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public ItemService(StoreLynkDbProd01Context context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Shared (CompanyId = 1) items plus the company's own, minus shared items the company has hidden
        private IQueryable<PTItem> VisibleItems(int companyId) =>
            _context.PTItems
            .Where(i => i.IsActive
                && (i.CompanyId == 1 || i.CompanyId == companyId)
                && !_context.PTCompanyItemOverrides.Any(o => o.CompanyId == companyId && o.ItemId == i.ItemId && o.IsHidden));

        private static Expression<Func<PTItem, PTItemDto>> ToItemDto(int companyId) => i => new PTItemDto
        {
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            IconName = i.IconName,
            IconUrl = i.IconUrl,
            Duration = i.Duration,
            IsDefault = i.IsDefault,
            CanEdit = i.CompanyId == companyId,
            ModifiedOn = i.ModifiedOn,
            Translations = i.Translations.Select(t => new TranslationDto
            {
                Locale = t.Locale,
                ItemName = t.ItemName
            }).ToList(),
            Categories = i.Categories.Select(c => c.CategoryId).ToList()
        };

        public async Task<List<PTItemDto>> GetItemsAsync(int companyId)
        {
            return await VisibleItems(companyId)
                .Select(ToItemDto(companyId))
                .ToListAsync();
        }


        public async Task<List<PTItemDto>> GetItemsAsync()
        {
            return await GetItemsAsync(GetCompanyIdFromToken());
        }

        public async Task<List<PTItemDto>> GetHiddenItemsAsync()
        {
            int companyId = GetCompanyIdFromToken();

            return await _context.PTItems
                .Where(i => i.IsActive && i.CompanyId == 1
                    && _context.PTCompanyItemOverrides.Any(o => o.CompanyId == companyId && o.ItemId == i.ItemId && o.IsHidden))
                .Select(ToItemDto(companyId))
                .ToListAsync();
        }
        public async Task AddItemAsync(ItemDto dto, IFormFile? ItemIcon)
        {

            int companyId = GetCompanyIdFromToken();

            // 1. Create the PTItem
            var item = new PTItem
            {
                ItemName = dto.Translations[0].ItemName,   // fallback / default
                Duration = dto.Duration,
                CompanyId = companyId,
                IsDefault = false,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                IsActive = true,
                IconName = SanitizeIconName(dto.Translations[0].ItemName)
            };

            if (dto.icon != null)
            {
                item.IconUrl = await UpLoadCompanyLogo(dto.icon);
            }
            await _context.PTItems.AddAsync(item);
            await _context.SaveChangesAsync();

            if (dto.Categories != null && dto.Categories.Any())
            {
                var categoryMappings = dto.Categories.Select(catId => new PTItemCategoryMapping
                {
                    ItemId = item.ItemId,
                    CategoryId = catId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                }).ToList();

                await _context.PTItemCategoryMapping.AddRangeAsync(categoryMappings);
            }

            // 4. Insert Translations
            if (dto.Translations != null && dto.Translations.Any())
            {
                var translationMappings = dto.Translations.Select(t => new PTItemTranslationMapping
                {
                    ItemId = item.ItemId,
                    Locale = t.Locale,
                    ItemName = t.ItemName,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                }).ToList();

                await _context.PTItemTranslationMapping.AddRangeAsync(translationMappings);
            }

            // 5. Save all changes
            await _context.SaveChangesAsync();
        }

        public async Task<ItemDto?> GetItemByIdAsync(int itemId)
        {
            int companyId = GetCompanyIdFromToken();

            var item = await VisibleItems(companyId)
                .Where(i => i.ItemId == itemId)
                .Select(i => new ItemDto
                {
                    ItemId = i.ItemId,
                    Duration = i.Duration,
                    IconUrl = i.IconUrl,
                    IconName = i.IconName,
                    IsDefault = i.IsDefault,
                    CanEdit = i.CompanyId == companyId,
                    Categories = _context.PTItemCategoryMapping
                        .Where(m => m.ItemId == i.ItemId)
                        .Select(m => m.CategoryId)
                        .ToList(),

                    Translations = _context.PTItemTranslationMapping
                        .Where(t => t.ItemId == i.ItemId)
                        .Select(t => new TranslationDto
                        {
                            Locale = t.Locale,
                            ItemName = t.ItemName
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return item;
        }

        public async Task<bool> UpdateItemAsync(ItemDto dto, IFormFile? ItemIcon)
        {
            int companyId = GetCompanyIdFromToken();

            // Only the owning company edits an item; shared items are read-only to other companies
            var existing = await _context.PTItems.FindAsync(dto.ItemId);
            if (existing == null || existing.CompanyId != companyId) return false;

            existing.ItemName = dto.ItemName;
            existing.Duration = dto.Duration;
            existing.ModifiedOn = DateTime.UtcNow;

            if (dto.Translations != null && dto.Translations.Any())
            {
                existing.ItemName = dto.Translations[0].ItemName;
            }

            existing.IconName = SanitizeIconName(existing.ItemName);

            if (dto.icon != null)
            {
                existing.IconUrl = await UpLoadCompanyLogo(dto.icon);
            }
            // If using a separate table for Store mappings, update logic here

            var existingCategories = _context.PTItemCategoryMapping
       .Where(x => x.ItemId == dto.ItemId);
            _context.PTItemCategoryMapping.RemoveRange(existingCategories);

            if (dto.Categories != null && dto.Categories.Any())
            {
                var categoryMappings = dto.Categories.Select(catId => new PTItemCategoryMapping
                {
                    ItemId = dto.ItemId.Value,
                    CategoryId = catId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                }).ToList();

                await _context.PTItemCategoryMapping.AddRangeAsync(categoryMappings);
            }

            // --- Update Translations ---
            var existingTranslations = _context.PTItemTranslationMapping
                .Where(x => x.ItemId == dto.ItemId);
            _context.PTItemTranslationMapping.RemoveRange(existingTranslations);

            if (dto.Translations != null && dto.Translations.Any())
            {
                var translationMappings = dto.Translations.Select(t => new PTItemTranslationMapping
                {
                    ItemId = dto.ItemId.Value,
                    Locale = t.Locale,
                    ItemName = t.ItemName,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                }).ToList();

                await _context.PTItemTranslationMapping.AddRangeAsync(translationMappings);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ItemDeleteResult> DeleteItemAsync(int id)
        {
            var (companyId, userId) = GetCallerFromToken();

            var item = await VisibleItems(companyId).FirstOrDefaultAsync(i => i.ItemId == id);
            if (item == null) return ItemDeleteResult.NotFound;

            // Another company deleting a shared item only hides it for that company
            if (item.CompanyId == 1 && companyId != 1)
            {
                var now = DateTime.UtcNow;
                var hidden = await _context.PTCompanyItemOverrides.FindAsync(companyId, id);
                if (hidden == null)
                {
                    _context.PTCompanyItemOverrides.Add(new PTCompanyItemOverride
                    {
                        CompanyId = companyId,
                        ItemId = id,
                        IsHidden = true,
                        CreatedBy = userId,
                        CreatedOn = now,
                        ModifiedBy = userId,
                        ModifiedOn = now
                    });
                }
                else
                {
                    hidden.IsHidden = true;
                    hidden.ModifiedBy = userId;
                    hidden.ModifiedOn = now;
                }

                await _context.SaveChangesAsync();
                return ItemDeleteResult.Hidden;
            }

            // Antunes removing a shared item: soft delete so timer history and reports keep it
            if (item.CompanyId == 1)
            {
                item.IsActive = false;
                item.ModifiedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return ItemDeleteResult.Deleted;
            }

            var mappings = _context.PTItemTranslationMapping
                            .Where(m => m.ItemId == id);

            _context.PTItemTranslationMapping.RemoveRange(mappings);

            await _context.SaveChangesAsync();

            var ctgmappings = _context.PTItemCategoryMapping
                          .Where(m => m.ItemId == id);

            _context.PTItemCategoryMapping.RemoveRange(ctgmappings);

            await _context.SaveChangesAsync();


            _context.PTItems.Remove(item);
            await _context.SaveChangesAsync();
            return ItemDeleteResult.Deleted;
        }

        public async Task<bool> RestoreItemAsync(int id)
        {
            var (companyId, userId) = GetCallerFromToken();

            var hidden = await _context.PTCompanyItemOverrides.FindAsync(companyId, id);
            if (hidden == null || !hidden.IsHidden) return false;

            // Keep the row if it also carries a duration override
            if (hidden.Duration == null)
            {
                _context.PTCompanyItemOverrides.Remove(hidden);
            }
            else
            {
                hidden.IsHidden = false;
                hidden.ModifiedBy = userId;
                hidden.ModifiedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateTestItem(CreateItemRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.ItemName) || request.CategoryIds == null || !request.CategoryIds.Any())
                return false;

            var now = DateTime.Now;

            // Create item
            var newItem = new PTItem
            {
                ItemName = request.ItemName,
                IconName = "TestIcon1",
                IconUrl = "https://slbackupstorage.blob.core.windows.net/storelynk/PrepTimer/ItemIcons/TestIcon1.jpg",
                Duration = 10,
                IsDefault = false,
                IsActive = true,
                CreatedOn = now,
                ModifiedOn = now
            };

            _context.PTItems.Add(newItem);
            await _context.SaveChangesAsync(); // Save to generate ItemId

            // Add translations with same name
            var locales = new[] { "en", "es", "fr", "zn", "pl", "de", "cz" };
            var translations = locales.Select(locale => new PTItemTranslationMapping
            {
                ItemId = newItem.ItemId,
                Locale = locale,
                ItemName = request.ItemName,
                CreatedOn = now,
                ModifiedOn = now
            }).ToList();

            _context.PTItemTranslationMapping.AddRange(translations);

            // Add categories
            var categoryMappings = request.CategoryIds.Select(catId => new PTItemCategoryMapping
            {
                ItemId = newItem.ItemId,
                CategoryId = catId,
                CreatedOn = now,
                ModifiedOn = now
            }).ToList();

            _context.PTItemCategoryMapping.AddRange(categoryMappings);

            await _context.SaveChangesAsync();

            return true;
        }
        private static string SanitizeIconName(string name) =>
            Regex.Replace(name?.Trim() ?? string.Empty, @"[^a-zA-Z0-9]+", "_").Trim('_');

        private async Task<string> UpLoadCompanyLogo(IFormFile ItemIcon)
        {
            string strUrl = string.Empty;
            try
            {
                String FileNameToUpload = String.Empty;

                string GUID = System.Guid.NewGuid().ToString();
                string extension = Path.GetExtension(ItemIcon.FileName);
                FileNameToUpload = GUID + "_" + Path.GetFileNameWithoutExtension(ItemIcon.FileName) + extension;

                string azureAccountName = _configuration.GetSection("Azure")["AccountName"];
                string azureAccountKey = _configuration.GetSection("Azure")["AccountKey"];

                string fname = "PrepTimer/ItemIcons/" + FileNameToUpload;

                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(azureAccountName, azureAccountKey);
                Uri blobUri = new Uri("https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + fname);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

                strUrl = "https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + fname;
                using (var stream = new MemoryStream())
                {
                    await ItemIcon.CopyToAsync(stream);
                    stream.Position = 0;
                    blobClient.Upload(stream);
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            return strUrl;
        }

        private int GetCompanyIdFromToken() => GetCallerFromToken().CompanyId;

        // TODO: a token whose user is not in PTUser falls back to company 1 (Antunes) - confirm how admins are identified
        private (int CompanyId, int? UserId) GetCallerFromToken()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) throw new UnauthorizedAccessException("User context not found.");

            var claim = user.Claims.FirstOrDefault(c => c.Type == "clientName");
            if (claim == null) throw new UnauthorizedAccessException("CompanyId not found in token.");
            var userDetails = _context.PTUsers.FirstOrDefault(a => a.Email.ToLower().Trim() == claim.Value.ToLower().Trim());

            var CompanyId = 1;
            if (userDetails != null)
                CompanyId = userDetails.CompanyId.Value;

            return (CompanyId, userDetails?.UserId);
        }
    }
}
