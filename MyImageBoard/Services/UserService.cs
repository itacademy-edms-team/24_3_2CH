using Microsoft.EntityFrameworkCore;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace MyImageBoard.Services;

public class UserService : IUserService
{
    private readonly ImageBoardContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ImageBoardContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            return await _context.Users
                .Include(u => u.Group)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user is null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {UserId}", id);
            throw;
        }
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
            {
                throw new KeyNotFoundException($"User with username {username} not found");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username {Username}", username);
            throw;
        }
    }

    public async Task<User> CreateUserAsync(User user, string password)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser is null)
            {
                throw new KeyNotFoundException($"User with ID {user.UserId} not found");
            }

            existingUser.Username = user.Username;
            existingUser.GroupId = user.GroupId;

            await _context.SaveChangesAsync();

            return existingUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", user.UserId);
            throw;
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null)
            {
                return false;
            }

            return VerifyPassword(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user credentials for {Username}", username);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsUserInGroupAsync(int userId, string groupName)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.Group?.Name == groupName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is in group {GroupName}", userId, groupName);
            throw;
        }
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Group)
                .ThenInclude(g => g.Permissions)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user?.Group?.Permissions is null)
            {
                return Enumerable.Empty<Permission>();
            }

            return user.Group.Permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionName)
    {
        try
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Any(p => p.Name == permissionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionName} for user {UserId}", permissionName, userId);
            throw;
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 