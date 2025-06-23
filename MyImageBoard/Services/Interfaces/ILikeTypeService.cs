using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface ILikeTypeService
    {
        Task<List<LikeType>> GetAllActiveLikeTypesAsync();
        Task<LikeType?> GetLikeTypeByIdAsync(int id);
        Task<LikeType?> GetLikeTypeByNameAsync(string name);
    }
} 