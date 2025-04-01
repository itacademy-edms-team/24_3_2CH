using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyImageBoard;

public partial class MyImageBoardContext : DbContext
{
    public MyImageBoardContext()
    {
    }

    public MyImageBoardContext(DbContextOptions<MyImageBoardContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Tread> Treads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MAXIMALLY;Database=MyImageBoard;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comment__3213E83FA595EDE9");

            entity.ToTable("Comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasMany(d => d.Images).WithMany(p => p.Comments)
                .UsingEntity<Dictionary<string, object>>(
                    "CommentImage",
                    r => r.HasOne<Image>().WithMany()
                        .HasForeignKey("ImageId")
                        .HasConstraintName("FK__Comment_I__image__45F365D3"),
                    l => l.HasOne<Comment>().WithMany()
                        .HasForeignKey("CommentId")
                        .HasConstraintName("FK__Comment_I__comme__44FF419A"),
                    j =>
                    {
                        j.HasKey("CommentId", "ImageId").HasName("PK__Comment___AA5CDA122E5551DC");
                        j.ToTable("Comment_Image");
                        j.IndexerProperty<int>("CommentId").HasColumnName("comment_id");
                        j.IndexerProperty<int>("ImageId").HasColumnName("image_id");
                    });
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image__3213E83F72363FBF");

            entity.ToTable("Image");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
        });

        modelBuilder.Entity<Tread>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tread__3213E83F5E07988C");

            entity.ToTable("Tread");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasMany(d => d.Comments).WithMany(p => p.Treads)
                .UsingEntity<Dictionary<string, object>>(
                    "TreadComment",
                    r => r.HasOne<Comment>().WithMany()
                        .HasForeignKey("CommentId")
                        .HasConstraintName("FK__Tread_Com__comme__3E52440B"),
                    l => l.HasOne<Tread>().WithMany()
                        .HasForeignKey("TreadId")
                        .HasConstraintName("FK__Tread_Com__tread__3D5E1FD2"),
                    j =>
                    {
                        j.HasKey("TreadId", "CommentId").HasName("PK__Tread_Co__8020AD766CEDC3FA");
                        j.ToTable("Tread_Comment");
                        j.IndexerProperty<int>("TreadId").HasColumnName("tread_id");
                        j.IndexerProperty<int>("CommentId").HasColumnName("comment_id");
                    });

            entity.HasMany(d => d.Images).WithMany(p => p.Treads)
                .UsingEntity<Dictionary<string, object>>(
                    "TreadImage",
                    r => r.HasOne<Image>().WithMany()
                        .HasForeignKey("ImageId")
                        .HasConstraintName("FK__Tread_Ima__image__4222D4EF"),
                    l => l.HasOne<Tread>().WithMany()
                        .HasForeignKey("TreadId")
                        .HasConstraintName("FK__Tread_Ima__tread__412EB0B6"),
                    j =>
                    {
                        j.HasKey("TreadId", "ImageId").HasName("PK__Tread_Im__A390568B1C45EF48");
                        j.ToTable("Tread_Image");
                        j.IndexerProperty<int>("TreadId").HasColumnName("tread_id");
                        j.IndexerProperty<int>("ImageId").HasColumnName("image_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
