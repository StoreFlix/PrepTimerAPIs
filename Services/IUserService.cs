using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;

namespace PrepTimerAPIs.Services
{
    public interface IUserService
    {
        Task<List<PTUser>> GetUsersAsync(int companyId);
        Task AddUserAsync(UserDto dto);
        Task<bool> UpdateUserAsync(PTUser user);
        Task<bool> DeleteUserAsync(int id);
        Task<String> ResetPassword(string password);
    }
}
