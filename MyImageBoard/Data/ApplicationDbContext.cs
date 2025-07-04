﻿using ForumProject.Data.Models; // Убедись, что этот using есть
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
        public DbSet<LikeType> LikeTypes { get; set; } = null!;
        public DbSet<Complaint> Complaints { get; set; } = null!;
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

            // Отключаем каскадное удаление для всех связей по умолчанию
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // Настраиваем каскадное удаление для комментариев
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.ChildComments)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Настраиваем каскадное удаление для зависимостей комментариев
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Comment)
                .WithMany(comment => comment.Complaints)
                .HasForeignKey(c => c.CommentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Comment)
                .WithMany(c => c.MediaFiles)
                .HasForeignKey(m => m.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настраиваем каскадное удаление для зависимостей тредов
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Thread)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Thread)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Thread)
                .WithMany(t => t.Complaints)
                .HasForeignKey(c => c.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Thread)
                .WithMany(t => t.MediaFiles)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настраиваем каскадное удаление для опросов
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Thread)
                .WithMany(t => t.Quizzes)
                .HasForeignKey(q => q.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizOption>()
                .HasOne(o => o.Quiz)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizVote>()
                .HasOne(v => v.QuizOption)
                .WithMany(o => o.Votes)
                .HasForeignKey(v => v.QuizOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Убеждаемся, что MediaFile привязан только к одному
            modelBuilder.Entity<MediaFile>()
                .HasCheckConstraint("CK_MediaFile_SingleParent",
                    "(ThreadId IS NULL AND CommentId IS NOT NULL) OR (ThreadId IS NOT NULL AND CommentId IS NULL)");

            // Уникальный индекс для лайков: один fingerprint - один лайк определенного типа на один тред/комментарий
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserFingerprintId, l.ThreadId, l.LikeTypeId })
                .HasFilter("[ThreadId] IS NOT NULL") // Индекс применяется только если ThreadId не NULL
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserFingerprintId, l.CommentId, l.LikeTypeId })
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