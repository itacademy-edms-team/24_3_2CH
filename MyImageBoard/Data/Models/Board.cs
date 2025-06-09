using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; } // Название доски (например, "Общие обсуждения", "Новости")

        [MaxLength(1000)]
        public string? Description { get; set; } // Описание доски

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационное свойство для тредов, которые принадлежат этой доске
        public ICollection<SiteThread> Threads { get; set; } = new List<SiteThread>();
    }
}