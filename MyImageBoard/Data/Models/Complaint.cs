using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        public int? ThreadId { get; set; } // FK к SiteThread, nullable
        public SiteThread? Thread { get; set; } // Навигационное свойство

        public int? CommentId { get; set; } // FK к Comment, nullable
        public Comment? Comment { get; set; } // Навигационное свойство

        public int FingerprintId { get; set; } // FK к UserFingerprint
        public UserFingerprint Fingerprint { get; set; } = null!; // Навигационное свойство

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty; // Причина жалобы
    }
}