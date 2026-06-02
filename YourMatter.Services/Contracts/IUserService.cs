using YourMatter.Data.Models;

namespace YourMatter.Services.Contracts
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<bool> UpdateProfileAsync(string id, string displayName, string? bio, string? location, string? profilePictureUrl);
        Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> SearchUsersAsync(string? searchTerm, string? sortBy, int page, int pageSize);
    }
}
