using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding; // Для [BindNever]

namespace ForumProject.Data.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Связь с тредом или комментарием
        public int? ThreadId { get; set; }
        [BindNever]
        public SiteThread? Thread { get; set; }

        public int? CommentId { get; set; }
        [BindNever]
        public Comment? Comment { get; set; }

        // Связь с UserFingerprint
        [Required] // Лайк всегда должен быть от какого-то отпечатка
        public int UserFingerprintId { get; set; }
        [BindNever] // Не привязываем UserFingerprint из формы
        public UserFingerprint UserFingerprint { get; set; } = null!; // Обязательное навигационное свойство
    }
}