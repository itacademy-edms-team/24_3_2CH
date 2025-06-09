using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    public class QuizOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; } // FK к Quiz
        public Quiz Quiz { get; set; } = null!; // Навигационное свойство

        [Required]
        public string Text { get; set; } = string.Empty;

        // Необязательно: счетчик голосов для быстрого отображения без запросов к QuizVote
        public int VotesCount { get; set; } = 0;

        public ICollection<QuizVote> Votes { get; set; } = new List<QuizVote>();
    }
}