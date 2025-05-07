using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NewImageBoard.Models;

public partial class ImageBoardContext : DbContext
{
    public ImageBoardContext()
    {
    }

    public ImageBoardContext(DbContextOptions<ImageBoardContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Board> Boards { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<ForumThread> Threads { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MAXIMALLY;Database=ImageBoard;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.BoardId).HasName("PK__Boards__F9646BD2FEA5A78E");

            entity.HasIndex(e => e.Name, "UQ__Boards__737584F683AC634F").IsUnique();

            entity.Property(e => e.BoardId).HasColumnName("BoardID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ShortName)
                .HasMaxLength(10)
                .HasDefaultValue("");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Boards)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Boards__CreatedB__47DBAE45");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__149AF30AEFB09943");

            entity.HasIndex(e => e.Name, "UQ__Groups__737584F6983C632C").IsUnique();

            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupPermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GroupPerm__Permi__3E52440B"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GroupPerm__Group__3D5E1FD2"),
                    j =>
                    {
                        j.HasKey("GroupId", "PermissionId").HasName("PK__GroupPer__FA609CBAB455859D");
                        j.ToTable("GroupPermissions");
                        j.IndexerProperty<int>("GroupId").HasColumnName("GroupID");
                        j.IndexerProperty<int>("PermissionId").HasColumnName("PermissionID");
                    });
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB0F36B6AE65");

            entity.HasIndex(e => e.Name, "UQ__Permissi__737584F6D9C71014").IsUnique();

            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__AA126038A071B040");

            entity.HasIndex(e => e.CreatedAt, "IX_Posts_CreatedAt");

            entity.HasIndex(e => e.ThreadId, "IX_Posts_ThreadID");

            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagePath).HasMaxLength(256);
            entity.Property(e => e.ThreadId).HasColumnName("ThreadID");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Posts__CreatedBy__5165187F");

            entity.HasOne(d => d.Thread).WithMany(p => p.Posts)
                .HasForeignKey(d => d.ThreadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Posts__ThreadID__5070F446");
        });

        modelBuilder.Entity<ForumThread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__Threads__688356E468EEB0C2");

            entity.HasIndex(e => e.BoardId, "IX_Threads_BoardID");

            entity.HasIndex(e => e.CreatedAt, "IX_Threads_CreatedAt");

            entity.Property(e => e.ThreadId).HasColumnName("ThreadID");
            entity.Property(e => e.BoardId).HasColumnName("BoardID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagePath).HasMaxLength(256);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Board).WithMany(p => p.Threads)
                .HasForeignKey(d => d.BoardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Threads__BoardID__4BAC3F29");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Threads)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Threads__Created__4CA06362");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACCD938E46");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4B3F2D4DF").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__GroupID__4316F928");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
