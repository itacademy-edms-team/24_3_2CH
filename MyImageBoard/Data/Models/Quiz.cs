using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; } // FK к SiteThread
        public SiteThread Thread { get; set; } = null!; // Навигационное свойство

        [Required]
        public string Question { get; set; } = string.Empty;

        [Required]
        public bool IsMultiple { get; set; } // Множественный выбор или единственный

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    }
}