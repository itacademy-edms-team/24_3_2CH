namespace ForumProject.Services.Interfaces
{
    public interface ILikeService
    {
        Task<int> AddLikeAsync(int userFingerprintId, int? threadId, int? commentId);
        Task<bool> RemoveLikeAsync(int userFingerprintId, int? threadId, int? commentId);
        Task<int> GetLikeCountAsync(int? threadId, int? commentId);
        Task<bool> HasUserLikedAsync(int userFingerprintId, int threadId, int? commentId);
        Task<bool> ToggleLikeAsync(int userFingerprintId, int? threadId, int? commentId);
    }
}