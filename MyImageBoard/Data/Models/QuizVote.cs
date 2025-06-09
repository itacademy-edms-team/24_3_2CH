using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ForumProject.Data.Models
{
    public class QuizVote
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int QuizOptionId { get; set; } // К какой опции относится голос
        [BindNever]
        public QuizOption QuizOption { get; set; } = null!;

        // Связь с UserFingerprint
        [Required] // Голос всегда должен быть от какого-то отпечатка
        public int UserFingerprintId { get; set; }
        [BindNever] // Не привязываем UserFingerprint из формы
        public UserFingerprint UserFingerprint { get; set; } = null!;
    }
}