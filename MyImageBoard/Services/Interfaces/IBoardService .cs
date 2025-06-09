using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface IBoardService
    {
        Task<Board?> GetBoardByIdAsync(int id);
        Task<IEnumerable<Board>> GetAllBoardsAsync();
        Task<Board> CreateBoardAsync(Board board);
        Task<bool> UpdateBoardAsync(Board board);
        Task<bool> DeleteBoardAsync(int id);
    }
}