using MyImageBoard.Models;

namespace MyImageBoard.Services.Interfaces;

public interface IThreadService
{
    Task<IEnumerable<ForumThread>> GetThreadsByBoardAsync(int boardId, int page = 1, int pageSize = 10);
    Task<ForumThread> GetThreadByIdAsync(int id);
    Task<ForumThread> CreateThreadAsync(ForumThread thread, int userId);
    Task<ForumThread> UpdateThreadAsync(ForumThread thread);
    Task DeleteThreadAsync(int id, int userId);
    Task<bool> IsThreadOwnerAsync(int threadId, int userId);
    Task<bool> CanUserModerateThreadAsync(int threadId, int userId);
    Task<IEnumerable<Post>> GetThreadPostsAsync(int threadId, int page = 1, int pageSize = 20);
    Task<int> GetThreadPostsCountAsync(int threadId);
} 