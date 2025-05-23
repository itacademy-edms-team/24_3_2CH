using MyImageBoard.Models;

namespace MyImageBoard.Services.Interfaces;

public interface IBoardService
{
    Task<IEnumerable<Board>> GetAllBoardsAsync();
    Task<Board> GetBoardByIdAsync(int id);
    Task<Board> GetBoardByShortNameAsync(string shortName);
    Task<Board> CreateBoardAsync(Board board, int userId);
    Task<Board> UpdateBoardAsync(Board board);
    Task DeleteBoardAsync(int id);
    Task<bool> IsUserModeratorAsync(int boardId, int userId);
    Task<bool> IsUserAdminAsync(int userId);
} 