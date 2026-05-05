using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;
using ServiceFabricApp.API.Repositories;
using ServiceFabricAPIsOld.Repositories;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PrepTimerAPIs.Services
{
    public class UserService : IUserService
    {
        private readonly StoreLynkDbProd01Context _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(StoreLynkDbProd01Context context, IConfiguration configuration, IEmailService emailService,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<PTUser>> GetUsersAsync(int companyId)
        {
            var CompanyId = GetCompanyIdFromToken();
            var items = await _context.PTUsers
                .Where(a => a.CompanyId == CompanyId
                            && (a.IsActive == null || a.IsActive == true))
                .OrderByDescending(a => a.ModifiedOn ?? a.CreatedOn)
                .ToListAsync();
            return items;
        }

        public async Task AddUserAsync(UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Email and Password are required.");

            var companyId = GetCompanyIdFromToken();

            var emailExists = await _context.PTUsers.AnyAsync(u =>
                u.CompanyId == companyId
                && u.Email.ToLower().Trim() == dto.Email.ToLower().Trim()
                && (u.IsActive == null || u.IsActive == true));

            if (emailExists)
                throw new InvalidOperationException("A user with this email already exists.");

            var user = new PTUser
            {
                LoginName = string.IsNullOrWhiteSpace(dto.LoginName) ? dto.Email : dto.LoginName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Password = Common.SFF_ENCRYPT(dto.Password),
                Phonenumber = dto.PhoneNumber,
                CompanyId = companyId,
                IsActive = true,
                CreatedOn = DateTime.Now,
                CreatedBy = GetCurrentUserId()
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
            existing.Phonenumber = user.Phonenumber;
            existing.ModifiedOn = DateTime.Now;
            existing.ModifiedBy = GetCurrentUserId();

            // Password is intentionally NOT updated here — use the reset-password flow.

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.PTUsers.FindAsync(id);
            if (user == null) return false;

            // Soft-delete: PTUser has FK collections (PtactiveDevices, PtcustomerSubscriptions)
            // so a hard delete would fail for any user with related rows.
            user.IsActive = false;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = GetCurrentUserId();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResponseResult> RequestPasswordResetAsync(string email)
        {
            // Avoid leaking which emails exist — always reply success.
            const string genericMessage = "If an account with that email exists, a reset link has been sent.";

            if (string.IsNullOrWhiteSpace(email))
                return ResponseResult.Success(genericMessage);

            var user = await _context.PTUsers
                .FirstOrDefaultAsync(x => x.Email.ToLower().Trim() == email.ToLower().Trim()
                                          && (x.IsActive == null || x.IsActive == true));
            if (user == null)
                return ResponseResult.Success(genericMessage);

            var tokenSvc = new TokenService(_configuration);
            var jwt = tokenSvc.GeneratePasswordResetToken(user.UserId, user.Email, user.Password);

            var frontendUrl = (_configuration["FrontendUrl"] ?? string.Empty).TrimEnd('/');
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(jwt)}";

            var body = $@"
                <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName ?? string.Empty)},</p>
                <p>We received a request to reset your password. Click the link below to choose a new one. This link is valid for 30 minutes and can only be used once.</p>
                <p><a href='{resetLink}'>Reset your password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                <p>— Antunes Prep Timer</p>";

            await _emailService.SendEmailAsync(user.Email, "Reset your Antunes Prep Timer password", body);

            return ResponseResult.Success(genericMessage);
        }

        public async Task<ResponseResult> ResetPasswordAsync(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                return ResponseResult.Failure("Invalid or expired link.");
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return ResponseResult.Failure("Password must be at least 6 characters.");

            var tokenSvc = new TokenService(_configuration);
            var principal = tokenSvc.ValidatePasswordResetToken(token);
            if (principal == null)
                return ResponseResult.Failure("Invalid or expired link.");

            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(subClaim, out var userId))
                return ResponseResult.Failure("Invalid or expired link.");

            var user = await _context.PTUsers.FindAsync(userId);
            if (user == null || user.IsActive == false)
                return ResponseResult.Failure("Invalid or expired link.");

            // Stamp binds the token to the password hash at issue time. If the password has
            // changed since (or if the link is being replayed after a successful reset), the
            // stamp won't match and the token is rejected.
            var stampClaim = principal.FindFirst("stamp")?.Value;
            if (!string.Equals(stampClaim, TokenService.BuildStamp(user.Password), StringComparison.Ordinal))
                return ResponseResult.Failure("This reset link has already been used or is no longer valid.");

            user.Password = Common.SFF_ENCRYPT(newPassword);
            user.ModifiedOn = DateTime.Now;
            // Clear any legacy Guid-based reset state if present.
            user.ResetPasswordToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return ResponseResult.Success("Password has been reset.");
        }

        public async Task<string> HashPassword(string password)
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

        public async Task<int> GetCompanyId()
        {
           return  GetCompanyIdFromToken();
        }

        private int GetCompanyIdFromToken()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) throw new UnauthorizedAccessException("User context not found.");

            var claim = user.Claims.FirstOrDefault(c => c.Type == "clientName");
            if (claim == null) throw new UnauthorizedAccessException("CompanyId not found in token.");
            var userDetails = _context.PTUsers.FirstOrDefault(a => a.Email.ToLower().Trim() == claim.Value.ToLower().Trim());

            var CompanyId = 1;
            if (userDetails != null)
                CompanyId = userDetails.CompanyId.Value;

            return CompanyId;
        }

        private int? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.Claims.FirstOrDefault(c => c.Type == "clientName");
            if (claim == null) return null;

            var userDetails = _context.PTUsers
                .FirstOrDefault(a => a.Email.ToLower().Trim() == claim.Value.ToLower().Trim());
            return userDetails?.UserId;
        }
    }
}
