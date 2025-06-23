using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class LikeType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = null!; // Эмодзи или символ реакции (например: 👍, ❤️, 😂, 😮, 😢, 😡)

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!; // Название реакции (например: "Like", "Heart", "Laugh", "Wow", "Sad", "Angry")

        [MaxLength(200)]
        public string? Description { get; set; } // Описание реакции (опционально)

        public bool IsActive { get; set; } = true; // Активна ли реакция

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационное свойство
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
} 