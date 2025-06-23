using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface ILikeTypeService
    {
        Task<List<LikeType>> GetAllActiveLikeTypesAsync();
        Task<LikeType?> GetLikeTypeByIdAsync(int id);
        Task<LikeType?> GetLikeTypeByNameAsync(string name);
        
        // Методы для управления типами реакций (только для Father)
        Task<(bool success, string message, LikeType? likeType)> CreateLikeTypeAsync(string symbol, string name, string? description = null);
        Task<(bool success, string message)> UpdateLikeTypeAsync(int id, string symbol, string name, string? description = null, bool? isActive = null);
        Task<(bool success, string message)> DeleteLikeTypeAsync(int id);
        Task<List<LikeType>> GetAllLikeTypesAsync(); // Включая неактивные
    }
} 