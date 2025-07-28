using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;
using System.ComponentModel.Design;
using System.Security.Cryptography;

namespace PrepTimerAPIs.Services
{
    public class UserService : IUserService
    {
        private readonly StoreLynkDbProd01Context _context;
        private readonly IConfiguration _configuration;


        public UserService(StoreLynkDbProd01Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<List<PTUser>> GetUsersAsync(int companyId)
        {
            var items = await _context.PTUsers.Where(a => a.CompanyId == null || a.CompanyId == companyId).ToListAsync();
            return items;
        }

        public async Task AddUserAsync(UserDto dto)
        {
            var user = new PTUser
            {
               LoginName = dto.Email,
               Email = dto.Email,
               FirstName = dto.FirstName,
               LastName = dto.LastName,
               Password = dto.Password,
               Phonenumber =    dto.Password,
                CompanyId = 1
            };

            await _context.PTUsers.AddAsync(user);
            await _context.SaveChangesAsync();
           
        }

        public async Task<bool> UpdateUserAsync(PTUser user)
        {
            var existing = await _context.PTUsers.FindAsync(user.UserId);
            if (existing == null) return false;

            existing.LoginName = user.LoginName;
            existing.Email = user.Email;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Password = user.Password;
            existing.Phonenumber = user.Phonenumber;

            // If using a separate table for Store mappings, update logic here

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.PTUsers.FindAsync(id);
            if (user == null) return false;

            _context.PTUsers.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> ResetPassword(string password)
        {
            byte[] KEY_64 = { 42, 16, 93, 156, 78, 4, 218, 32 };
            byte[] IV_64 = { 55, 103, 246, 79, 36, 99, 167, 3 };
            if (!string.IsNullOrEmpty(password))
            {
                try
                {
                    // value = MiscFunctions.SFF_REPLACE_STRING(value, "-", "+");

                    using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
                    {
                        // Convert from string to byte array
                        byte[] buffer = Convert.FromBase64String(password);

                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read))
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception)
                {
                    return "INVALID";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
