using Microsoft.EntityFrameworkCore;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;

namespace MyImageBoard.Services;

public class BoardService : IBoardService
{
    private readonly ImageBoardContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<BoardService> _logger;

    public BoardService(
        ImageBoardContext context,
        IUserService userService,
        ILogger<BoardService> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task<IEnumerable<Board>> GetAllBoardsAsync()
    {
        try
        {
            return await _context.Boards
                .Include(b => b.CreatedByNavigation)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all boards");
            throw;
        }
    }

    public async Task<Board> GetBoardByIdAsync(int id)
    {
        try
        {
            var board = await _context.Boards
                .Include(b => b.CreatedByNavigation)
                .FirstOrDefaultAsync(b => b.BoardId == id);

            if (board is null)
            {
                throw new KeyNotFoundException($"Board with ID {id} not found");
            }

            return board;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting board by ID {BoardId}", id);
            throw;
        }
    }

    public async Task<Board> GetBoardByShortNameAsync(string shortName)
    {
        try
        {
            var board = await _context.Boards
                .Include(b => b.CreatedByNavigation)
                .FirstOrDefaultAsync(b => b.ShortName == shortName);

            if (board is null)
            {
                throw new KeyNotFoundException($"Board with short name {shortName} not found");
            }

            return board;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting board by short name {ShortName}", shortName);
            throw;
        }
    }

    public async Task<Board> CreateBoardAsync(Board board, int userId)
    {
        try
        {
            if (!await _userService.HasPermissionAsync(userId, "CreateBoard"))
            {
                throw new UnauthorizedAccessException("User does not have permission to create boards");
            }

            board.CreatedBy = userId;
            board.CreatedAt = DateTime.UtcNow;

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return board;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating board");
            throw;
        }
    }

    public async Task<Board> UpdateBoardAsync(Board board)
    {
        try
        {
            var existingBoard = await _context.Boards.FindAsync(board.BoardId);
            if (existingBoard is null)
            {
                throw new KeyNotFoundException($"Board with ID {board.BoardId} not found");
            }

            if (!await _userService.HasPermissionAsync(board.CreatedBy, "ModifyBoard"))
            {
                throw new UnauthorizedAccessException("User does not have permission to modify boards");
            }

            existingBoard.Name = board.Name;
            existingBoard.ShortName = board.ShortName;
            existingBoard.Description = board.Description;

            await _context.SaveChangesAsync();

            return existingBoard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating board {BoardId}", board.BoardId);
            throw;
        }
    }

    public async Task DeleteBoardAsync(int id)
    {
        try
        {
            var board = await _context.Boards.FindAsync(id);
            if (board is null)
            {
                throw new KeyNotFoundException($"Board with ID {id} not found");
            }

            if (!await _userService.HasPermissionAsync(board.CreatedBy, "DeleteBoard"))
            {
                throw new UnauthorizedAccessException("User does not have permission to delete boards");
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board {BoardId}", id);
            throw;
        }
    }

    public async Task<bool> IsUserModeratorAsync(int boardId, int userId)
    {
        try
        {
            return await _userService.HasPermissionAsync(userId, "ModerateBoard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is moderator for board {BoardId}", userId, boardId);
            throw;
        }
    }

    public async Task<bool> IsUserAdminAsync(int userId)
    {
        try
        {
            return await _userService.HasPermissionAsync(userId, "Admin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is admin", userId);
            throw;
        }
    }
} 