using ForumProject.Data.Models; // Убедись, что этот using есть
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSet для всех наших моделей ---
        public DbSet<SiteThread> Threads { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<MediaFile> MediaFiles { get; set; } = null!;
        public DbSet<UserFingerprint> UserFingerprints { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Complaint> Complaints { get; set; } = null!;
        public DbSet<Moderator> Moderators { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RoleGroup> RoleGroups { get; set; } = null!;
        public DbSet<ModeratorRoleGroup> ModeratorRoleGroups { get; set; } = null!;
        public DbSet<RoleGroupRole> RoleGroupRoles { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizOption> QuizOptions { get; set; } = null!;
        public DbSet<QuizVote> QuizVotes { get; set; } = null!;
        public DbSet<Board> Boards { get; set; } = null!;

        // New DbSets for superuser functionality
        public DbSet<SuperUser> SuperUsers { get; set; } = null!;
        public DbSet<SuperUserGroup> SuperUserGroups { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<SuperUserPermission> SuperUserPermissions { get; set; } = null!;
        public DbSet<SuperUserGroupPermission> SuperUserGroupPermissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Отключаем каскадное удаление для всех связей
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // Убеждаемся, что MediaFile привязан только к одному
            modelBuilder.Entity<MediaFile>()
                .HasCheckConstraint("CK_MediaFile_SingleParent",
                    "(ThreadId IS NULL AND CommentId IS NOT NULL) OR (ThreadId IS NOT NULL AND CommentId IS NULL)");

            // Уникальный индекс для лайков: один fingerprint - один лайк на один тред/комментарий
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserFingerprintId, l.ThreadId })
                .HasFilter("[ThreadId] IS NOT NULL") // Индекс применяется только если ThreadId не NULL
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserFingerprintId, l.CommentId })
                .HasFilter("[CommentId] IS NOT NULL") // Индекс применяется только если CommentId не NULL
                .IsUnique();

            // Unique index for FingerprintHash in UserFingerprint
            modelBuilder.Entity<UserFingerprint>()
                .HasIndex(uf => uf.FingerprintHash)
                .IsUnique();

            // Уникальный индекс для QuizVote: один fingerprint - один голос на один Quiz
            modelBuilder.Entity<QuizVote>()
                .HasIndex(qv => new { qv.UserFingerprintId, qv.QuizOptionId })
                .IsUnique();

            // Настройка связующих таблиц для "многие ко многим"
            modelBuilder.Entity<ModeratorRoleGroup>()
                .HasKey(mrg => new { mrg.ModeratorId, mrg.RoleGroupId }); // Составной первичный ключ

            modelBuilder.Entity<ModeratorRoleGroup>()
                .HasOne(mrg => mrg.Moderator)
                .WithMany(m => m.ModeratorRoleGroups)
                .HasForeignKey(mrg => mrg.ModeratorId);

            modelBuilder.Entity<ModeratorRoleGroup>()
                .HasOne(mrg => mrg.RoleGroup)
                .WithMany(rg => rg.ModeratorRoleGroups)
                .HasForeignKey(mrg => mrg.RoleGroupId);

            modelBuilder.Entity<RoleGroupRole>()
                .HasKey(rgr => new { rgr.RoleGroupId, rgr.RoleId }); // Составной первичный ключ

            modelBuilder.Entity<RoleGroupRole>()
                .HasOne(rgr => rgr.RoleGroup)
                .WithMany(rg => rg.RoleGroupRoles)
                .HasForeignKey(rgr => rgr.RoleGroupId);

            modelBuilder.Entity<RoleGroupRole>()
                .HasOne(rgr => rgr.Role)
                .WithMany(r => r.RoleGroupRoles)
                .HasForeignKey(rgr => rgr.RoleId);

            // SuperUser -> Group
            modelBuilder.Entity<SuperUser>()
                .HasOne(su => su.Group)
                .WithMany(g => g.SuperUsers)
                .HasForeignKey(su => su.SuperUserGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            // SuperUser -> Permission (many-to-many)
            modelBuilder.Entity<SuperUserPermission>()
                .HasOne(sup => sup.SuperUser)
                .WithMany(su => su.SuperUserPermissions)
                .HasForeignKey(sup => sup.SuperUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SuperUserPermission>()
                .HasOne(sup => sup.Permission)
                .WithMany(p => p.SuperUserPermissions)
                .HasForeignKey(sup => sup.PermissionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Group -> Permission (many-to-many)
            modelBuilder.Entity<SuperUserGroupPermission>()
                .HasOne(sugp => sugp.SuperUserGroup)
                .WithMany(sug => sug.GroupPermissions)
                .HasForeignKey(sugp => sugp.SuperUserGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SuperUserGroupPermission>()
                .HasOne(sugp => sugp.Permission)
                .WithMany(p => p.GroupPermissions)
                .HasForeignKey(sugp => sugp.PermissionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}