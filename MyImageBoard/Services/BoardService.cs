using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services
{
    public class BoardService : IBoardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IThreadService _threadService; // Для рекурсивного удаления тредов

        // Добавляем IThreadService через Dependency Injection
        public BoardService(ApplicationDbContext context, IThreadService threadService)
        {
            _context = context;
            _threadService = threadService;
        }

        public async Task<Board?> GetBoardByIdAsync(int id)
        {
            return await _context.Boards
                .Include(b => b.Threads)
                    .ThenInclude(t => t.MediaFiles)
                .Include(b => b.Threads)
                    .ThenInclude(t => t.Comments)
                .Include(b => b.Threads)
                    .ThenInclude(t => t.Quizzes)
                        .ThenInclude(q => q.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Board>> GetAllBoardsAsync()
        {
            return await _context.Boards
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Board> CreateBoardAsync(Board board)
        {
            board.CreatedAt = DateTime.UtcNow;
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<bool> UpdateBoardAsync(Board board)
        {
            var existingBoard = await _context.Boards.FindAsync(board.Id);
            if (existingBoard == null)
            {
                return false;
            }

            existingBoard.Title = board.Title;
            existingBoard.Description = board.Description;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBoardAsync(int id)
        {
            var board = await _context.Boards
                .Include(b => b.Threads)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board == null)
            {
                return false;
            }

            // Удаляем все треды доски (это автоматически удалит все связанные сущности через ThreadService)
            foreach (var thread in board.Threads)
            {
                await _threadService.DeleteThreadAsync(thread.Id);
            }

            // Удаляем саму доску
            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}