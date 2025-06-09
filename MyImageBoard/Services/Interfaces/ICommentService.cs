using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface ICommentService
    {
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<IEnumerable<Comment>> GetCommentsForThreadAsync(int threadId); // Получить корневые комментарии треда
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int id, bool saveChanges = true); // Hard delete, включая вложенные
        // Добавим методы для обработки жалоб и медиа, когда дойдем до них
    }
}