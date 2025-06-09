using ForumProject.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
                {
                    new Permission { Name = "DeleteThread", Description = "Can delete threads" },
                    new Permission { Name = "DeleteComment", Description = "Can delete comments" },
                    new Permission { Name = "DeleteComplaint", Description = "Can delete complaints" },
                    new Permission { Name = "CreateBoard", Description = "Can create boards" },
                    new Permission { Name = "DeleteBoard", Description = "Can delete boards" },
                    new Permission { Name = "CreateSuperUser", Description = "Can create new superusers" },
                    new Permission { Name = "BlockSuperUser", Description = "Can block superusers" },
                    new Permission { Name = "ManageSuperUserPermissions", Description = "Can manage superuser permissions" }
                };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            if (!context.SuperUserGroups.Any())
            {
                var moderatorGroup = new SuperUserGroup
                {
                    Name = "Moderator",
                    Description = "Can moderate threads, comments and complaints"
                };

                var adminGroup = new SuperUserGroup
                {
                    Name = "Admin",
                    Description = "Can manage boards and moderators"
                };

                var fatherGroup = new SuperUserGroup
                {
                    Name = "Father",
                    Description = "Has full control over the system"
                };

                await context.SuperUserGroups.AddRangeAsync(new[] { moderatorGroup, adminGroup, fatherGroup });
                await context.SaveChangesAsync();

                // Assign permissions to groups
                var allPermissions = await context.Permissions.ToListAsync();
                var moderatorPermissions = allPermissions.Where(p => 
                    p.Name == "DeleteThread" || 
                    p.Name == "DeleteComment" || 
                    p.Name == "DeleteComplaint"
                ).ToList();

                var adminPermissions = allPermissions.Where(p =>
                    p.Name == "DeleteThread" ||
                    p.Name == "DeleteComment" ||
                    p.Name == "DeleteComplaint" ||
                    p.Name == "CreateBoard" ||
                    p.Name == "DeleteBoard" ||
                    p.Name == "CreateSuperUser" ||
                    p.Name == "BlockSuperUser"
                ).ToList();

                // Father gets all permissions
                var fatherPermissions = allPermissions;

                foreach (var permission in moderatorPermissions)
                {
                    await context.SuperUserGroupPermissions.AddAsync(new SuperUserGroupPermission
                    {
                        SuperUserGroupId = moderatorGroup.Id,
                        PermissionId = permission.Id
                    });
                }

                foreach (var permission in adminPermissions)
                {
                    await context.SuperUserGroupPermissions.AddAsync(new SuperUserGroupPermission
                    {
                        SuperUserGroupId = adminGroup.Id,
                        PermissionId = permission.Id
                    });
                }

                foreach (var permission in fatherPermissions)
                {
                    await context.SuperUserGroupPermissions.AddAsync(new SuperUserGroupPermission
                    {
                        SuperUserGroupId = fatherGroup.Id,
                        PermissionId = permission.Id
                    });
                }

                await context.SaveChangesAsync();

                // Create initial Father superuser
                var passwordHasher = new PasswordHasher<SuperUser>();
                var father = new SuperUser
                {
                    Username = "father",
                    SuperUserGroupId = fatherGroup.Id,
                    CreatedAt = DateTime.UtcNow
                };
                father.PasswordHash = passwordHasher.HashPassword(father, "Admin123!"); // Default password

                await context.SuperUsers.AddAsync(father);
                await context.SaveChangesAsync();

                // Assign all permissions directly to Father
                foreach (var permission in fatherPermissions)
                {
                    await context.SuperUserPermissions.AddAsync(new SuperUserPermission
                    {
                        SuperUserId = father.Id,
                        PermissionId = permission.Id
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
} 