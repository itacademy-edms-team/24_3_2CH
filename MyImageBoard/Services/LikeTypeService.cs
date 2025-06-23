using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services
{
    public class LikeTypeService : ILikeTypeService
    {
        private readonly ApplicationDbContext _context;

        public LikeTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LikeType>> GetAllActiveLikeTypesAsync()
        {
            return await _context.LikeTypes
                .Where(lt => lt.IsActive)
                .OrderBy(lt => lt.Id)
                .ToListAsync();
        }

        public async Task<LikeType?> GetLikeTypeByIdAsync(int id)
        {
            return await _context.LikeTypes
                .FirstOrDefaultAsync(lt => lt.Id == id && lt.IsActive);
        }

        public async Task<LikeType?> GetLikeTypeByNameAsync(string name)
        {
            return await _context.LikeTypes
                .FirstOrDefaultAsync(lt => lt.Name == name && lt.IsActive);
        }
    }
} 