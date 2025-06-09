using ForumProject.Data;
using ForumProject.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services
{
    public interface ISuperUserService
    {
        Task<(bool success, string message, SuperUser? user)> CreateSuperUserAsync(string username, string password, int groupId, List<string>? customPermissions = null, int? creatorId = null);
        Task<(bool success, string message)> ToggleBlockStatusAsync(int userId, int requesterId);
        Task<(bool success, string message, SuperUser? user)> AuthenticateAsync(string username, string password);
        Task<List<Permission>> GetUserPermissionsAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permissionName);
        Task<List<SuperUser>> GetAllSuperUsersAsync();
        Task<SuperUser?> GetSuperUserByIdAsync(int id);
        Task<List<Permission>> GetAvailablePermissionsAsync();
        Task<List<SuperUserGroup>> GetAvailableGroupsAsync();
    }

    public class SuperUserService : ISuperUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<SuperUser> _passwordHasher;

        public SuperUserService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<SuperUser>();
        }

        public async Task<(bool success, string message, SuperUser? user)> CreateSuperUserAsync(
            string username, 
            string password, 
            int groupId, 
            List<string>? customPermissions = null,
            int? creatorId = null)
        {
            // Проверка входных данных
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (false, "Username and password are required.", null);

            // Проверка существования пользователя
            if (await _context.SuperUsers.AnyAsync(su => su.Username == username))
                return (false, "Username already exists.", null);

            // Проверка существования группы
            var group = await _context.SuperUserGroups
                .Include(g => g.GroupPermissions)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return (false, "Invalid group selected.", null);

            // Если указан creatorId, проверяем права создателя
            if (creatorId.HasValue)
            {
                var creator = await _context.SuperUsers
                    .Include(su => su.Group)
                    .FirstOrDefaultAsync(su => su.Id == creatorId);

                if (creator == null)
                    return (false, "Creator not found.", null);

                // Проверка прав на создание суперпользователей
                if (creator.Group.Name != "Father" && !await HasPermissionAsync(creatorId.Value, "CreateSuperUser"))
                    return (false, "Insufficient permissions to create superuser.", null);

                // Только Father может создавать админов
                if (group.Name == "Admin" && creator.Group.Name != "Father")
                    return (false, "Only Father can create Admin users.", null);
            }

            // Создание нового суперпользователя
            var newUser = new SuperUser
            {
                Username = username,
                SuperUserGroupId = groupId,
                CreatedAt = DateTime.UtcNow
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

            await _context.SuperUsers.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Добавление кастомных разрешений (только если создатель - Father)
            if (creatorId.HasValue && customPermissions != null)
            {
                var creator = await _context.SuperUsers
                    .Include(su => su.Group)
                    .FirstOrDefaultAsync(su => su.Id == creatorId);

                if (creator?.Group.Name == "Father")
                {
                    foreach (var permissionName in customPermissions)
                    {
                        var permission = await _context.Permissions
                            .FirstOrDefaultAsync(p => p.Name == permissionName);

                        if (permission != null)
                        {
                            await _context.SuperUserPermissions.AddAsync(new SuperUserPermission
                            {
                                SuperUserId = newUser.Id,
                                PermissionId = permission.Id
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return (true, "Superuser created successfully.", newUser);
        }

        public async Task<(bool success, string message)> ToggleBlockStatusAsync(int userId, int requesterId)
        {
            // Получаем пользователя, которого нужно заблокировать/разблокировать
            var user = await _context.SuperUsers
                .Include(su => su.Group)
                .FirstOrDefaultAsync(su => su.Id == userId);

            if (user == null)
                return (false, "User not found.");

            // Получаем пользователя, который делает запрос
            var requester = await _context.SuperUsers
                .Include(su => su.Group)
                .FirstOrDefaultAsync(su => su.Id == requesterId);

            if (requester == null)
                return (false, "Requester not found.");

            // Проверяем права
            if (requester.Group.Name != "Father" && !await HasPermissionAsync(requesterId, "BlockSuperUser"))
                return (false, "Insufficient permissions to block/unblock users.");

            // Нельзя блокировать Father или самого себя
            if (user.Group.Name == "Father" || user.Id == requesterId)
                return (false, "Cannot block this user.");

            // Только Father может блокировать админов
            if (user.Group.Name == "Admin" && requester.Group.Name != "Father")
                return (false, "Only Father can block Admin users.");

            user.IsBlocked = !user.IsBlocked;
            await _context.SaveChangesAsync();

            return (true, $"User {user.Username} has been {(user.IsBlocked ? "blocked" : "unblocked")}.");
        }

        public async Task<(bool success, string message, SuperUser? user)> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (false, "Username and password are required.", null);

            var user = await _context.SuperUsers
                .Include(su => su.Group)
                .FirstOrDefaultAsync(su => su.Username == username);

            if (user == null)
                return (false, "Invalid username or password.", null);

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                return (false, "Invalid username or password.", null);

            if (user.IsBlocked)
                return (false, "Your account has been blocked.", null);

            return (true, "Authentication successful.", user);
        }

        public async Task<List<Permission>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.SuperUsers
                .Include(su => su.Group)
                .FirstOrDefaultAsync(su => su.Id == userId);

            if (user == null)
                return new List<Permission>();

            if (user.Group.Name == "Father")
                return await _context.Permissions.ToListAsync();

            var permissions = await _context.Permissions
                .Where(p =>
                    p.SuperUserPermissions.Any(sup => sup.SuperUserId == userId) ||
                    p.GroupPermissions.Any(gp => gp.SuperUserGroupId == user.SuperUserGroupId)
                )
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            var user = await _context.SuperUsers
                .Include(su => su.Group)
                .FirstOrDefaultAsync(su => su.Id == userId);

            if (user == null)
                return false;

            if (user.Group.Name == "Father")
                return true;

            return await _context.Permissions
                .AnyAsync(p =>
                    p.Name == permissionName &&
                    (p.SuperUserPermissions.Any(sup => sup.SuperUserId == userId) ||
                     p.GroupPermissions.Any(gp => gp.SuperUserGroupId == user.SuperUserGroupId))
                );
        }

        public async Task<List<SuperUser>> GetAllSuperUsersAsync()
        {
            return await _context.SuperUsers
                .Include(su => su.Group)
                .OrderBy(su => su.Username)
                .ToListAsync();
        }

        public async Task<SuperUser?> GetSuperUserByIdAsync(int id)
        {
            return await _context.SuperUsers
                .Include(su => su.Group)
                .FirstOrDefaultAsync(su => su.Id == id);
        }

        public async Task<List<Permission>> GetAvailablePermissionsAsync()
        {
            return await _context.Permissions
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<SuperUserGroup>> GetAvailableGroupsAsync()
        {
            return await _context.SuperUserGroups
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
    }
} 