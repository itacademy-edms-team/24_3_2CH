using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    public class UserFingerprint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)] // Или другое подходящее значение для хеша
        public string FingerprintHash { get; set; } = null!; // Хеш отпечатка пользователя

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства для связей (только если они нужны, но для отпечатка лучше не держать лишние)
        // public ICollection<Like> Likes { get; set; } = new List<Like>();
        // public ICollection<QuizVote> QuizVotes { get; set; } = new List<QuizVote>();
        // Убрал прямые коллекции здесь, они будут в моделях Like и QuizVote
    }
}