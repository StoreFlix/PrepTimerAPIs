using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.Extensions.Configuration;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Dtos;
using Microsoft.EntityFrameworkCore;

namespace PrepTimerAPIs.Services
{
    public class ItemService :IItemService
    {

        private readonly StoreLynkDbProd01Context _context;
        private readonly IConfiguration _configuration;


        public ItemService(StoreLynkDbProd01Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; 
        }

        public async Task<List<PTItemDto>> GetItemsAsync(int companyId)
        {
            var items = await _context.PTItems
            .Include(i => i.Translations)
            .Include(i => i.Categories)
            .Where(i => i.IsActive)
            .Select(i => new PTItemDto
            {
                ItemId = i.ItemId,
                ItemName = i.ItemName,
                IconName = i.IconName,
                IconUrl = i.IconUrl,
                Duration = i.Duration,
                IsDefault = i.IsDefault,
                ModifiedOn = i.ModifiedOn,
                Translations = i.Translations.Select(t => new TranslationDto
                {
                    Locale = t.Locale,
                    ItemName = t.ItemName
                }).ToList(),
                Categories = i.Categories.Select(c => c.CategoryId).ToList()
            })
            .ToListAsync();

            return items;

        }

        public async Task AddItemAsync(ItemDto dto, IFormFile? ItemIcon)
        {
            var item = new PTItem
            {
                ItemName = dto.ItemName,
                Duration = dto.Duration,
                CompanyId = 1,
                IsDefault = false,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };

            if (ItemIcon != null)
            {
                item.IconName=ItemIcon.Name;
                item.IconUrl = await UpLoadCompanyLogo(ItemIcon);
            }

            await _context.PTItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateItemAsync(ItemDto item, IFormFile? ItemIcon)
        {
            var existing = await _context.PTItems.FindAsync(item.ItemId);
            if (existing == null) return false;

            existing.ItemName = item.ItemName;
            existing.Duration = item.Duration;
            existing.ModifiedOn = DateTime.UtcNow;

            if (ItemIcon != null)
            {
                existing.IconName = ItemIcon.Name;
                existing.IconUrl = await UpLoadCompanyLogo(ItemIcon);
            }
            // If using a separate table for Store mappings, update logic here

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _context.PTItems.FindAsync(id);
            if (item == null) return false;

            _context.PTItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
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
    }
}
