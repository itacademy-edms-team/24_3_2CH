using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;

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

        public async Task<(bool success, string message, LikeType? likeType)> CreateLikeTypeAsync(string symbol, string name, string? description = null)
        {
            Console.WriteLine($"CreateLikeTypeAsync called with: symbol='{symbol}', name='{name}', description='{description}'");
            
            if (string.IsNullOrWhiteSpace(symbol))
            {
                Console.WriteLine("Symbol is null or whitespace");
                return (false, "Символ реакции не может быть пустым.", null);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name is null or whitespace");
                return (false, "Название реакции не может быть пустым.", null);
            }

            if (symbol.Length > 10)
            {
                Console.WriteLine($"Symbol too long: {symbol.Length}");
                return (false, "Символ реакции не может быть длиннее 10 символов.", null);
            }

            if (name.Length > 50)
            {
                Console.WriteLine($"Name too long: {name.Length}");
                return (false, "Название реакции не может быть длиннее 50 символов.", null);
            }

            // Проверяем, не существует ли уже такой символ или название
            if (await _context.LikeTypes.AnyAsync(lt => lt.Symbol == symbol))
            {
                Console.WriteLine($"Symbol already exists: {symbol}");
                return (false, "Реакция с таким символом уже существует.", null);
            }

            if (await _context.LikeTypes.AnyAsync(lt => lt.Name == name))
            {
                Console.WriteLine($"Name already exists: {name}");
                return (false, "Реакция с таким названием уже существует.", null);
            }

            Console.WriteLine("All validations passed, creating new LikeType");
            
            try
            {
                var newLikeType = new LikeType
                {
                    Symbol = symbol,
                    Name = name,
                    Description = description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"About to add LikeType to context: {newLikeType.Symbol}");
                await _context.LikeTypes.AddAsync(newLikeType);
                Console.WriteLine("LikeType added to context, calling SaveChangesAsync");
                await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync completed successfully");

                Console.WriteLine($"LikeType created successfully with ID: {newLikeType.Id}");

                return (true, "Тип реакции успешно создан.", newLikeType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while creating LikeType: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return (false, $"Ошибка при создании типа реакции: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> UpdateLikeTypeAsync(int id, string symbol, string name, string? description = null, bool? isActive = null)
        {
            var likeType = await _context.LikeTypes.FindAsync(id);
            if (likeType == null)
                return (false, "Тип реакции не найден.");

            if (string.IsNullOrWhiteSpace(symbol))
                return (false, "Символ реакции не может быть пустым.");

            if (string.IsNullOrWhiteSpace(name))
                return (false, "Название реакции не может быть пустым.");

            if (symbol.Length > 10)
                return (false, "Символ реакции не может быть длиннее 10 символов.");

            if (name.Length > 50)
                return (false, "Название реакции не может быть длиннее 50 символов.");

            // Проверяем, не существует ли уже такой символ или название (исключая текущий)
            if (await _context.LikeTypes.AnyAsync(lt => lt.Symbol == symbol && lt.Id != id))
                return (false, "Реакция с таким символом уже существует.");

            if (await _context.LikeTypes.AnyAsync(lt => lt.Name == name && lt.Id != id))
                return (false, "Реакция с таким названием уже существует.");

            likeType.Symbol = symbol;
            likeType.Name = name;
            likeType.Description = description;
            if (isActive.HasValue)
                likeType.IsActive = isActive.Value;

            await _context.SaveChangesAsync();

            return (true, "Тип реакции успешно обновлен.");
        }

        public async Task<(bool success, string message)> DeleteLikeTypeAsync(int id)
        {
            var likeType = await _context.LikeTypes.FindAsync(id);
            if (likeType == null)
                return (false, "Тип реакции не найден.");

            // Проверяем, есть ли лайки с этим типом
            var hasLikes = await _context.Likes.AnyAsync(l => l.LikeTypeId == id);
            if (hasLikes)
                return (false, "Нельзя удалить тип реакции, который используется в лайках.");

            _context.LikeTypes.Remove(likeType);
            await _context.SaveChangesAsync();

            return (true, "Тип реакции успешно удален.");
        }

        public async Task<List<LikeType>> GetAllLikeTypesAsync()
        {
            return await _context.LikeTypes
                .OrderBy(lt => lt.Id)
                .ToListAsync();
        }
    }
} 