using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface ILikeService
    {
        Task<int> AddLikeAsync(int userFingerprintId, int? threadId, int? commentId, int likeTypeId);
        Task<bool> RemoveLikeAsync(int userFingerprintId, int? threadId, int? commentId, int likeTypeId);
        Task<int> GetLikeCountAsync(int? threadId, int? commentId, int? likeTypeId = null);
        Task<bool> HasUserLikedAsync(int userFingerprintId, int threadId, int? commentId, int likeTypeId);
        Task<bool> ToggleLikeAsync(int userFingerprintId, int? threadId, int? commentId, int likeTypeId);
        Task<Dictionary<int, int>> GetLikeCountsByTypeAsync(int? threadId, int? commentId);
        
        // Новые методы для работы с реакциями по типам
        Task<Dictionary<int, bool>> GetUserReactionsForThreadAsync(int userFingerprintId, int threadId);
        Task<Dictionary<int, int>> GetReactionCountsForThreadAsync(int threadId);
        Task<Dictionary<int, Dictionary<int, bool>>> GetUserReactionsForCommentsAsync(int userFingerprintId, ICollection<Comment> comments);
        Task<Dictionary<int, Dictionary<int, int>>> GetReactionCountsForCommentsAsync(ICollection<Comment> comments);
    }
}