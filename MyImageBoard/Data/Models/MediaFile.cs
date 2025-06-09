using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    public class MediaFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public required string FileName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FileType { get; set; }

        [Required]
        [MaxLength(100)]
        public required string MimeType { get; set; }

        public long Size { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Связь с тредом или комментарием (один из них должен быть null)
        public int? ThreadId { get; set; }
        public SiteThread? Thread { get; set; }

        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }

        [NotMapped]
        public bool IsValidReference => (ThreadId.HasValue ^ CommentId.HasValue);
    }
}