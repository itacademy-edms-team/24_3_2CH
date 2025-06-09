using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class QuizCreateRequest
    {
        [Required(ErrorMessage = "Вопрос обязателен")]
        [StringLength(500, ErrorMessage = "Вопрос не может быть длиннее 500 символов")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Необходимо указать тип опроса")]
        public bool IsMultiple { get; set; }

        [Required(ErrorMessage = "Необходимо указать варианты ответа")]
        [MinLength(2, ErrorMessage = "Минимальное количество вариантов ответа: 2")]
        [MaxLength(42, ErrorMessage = "Максимальное количество вариантов ответа: 42")]
        public List<string> Options { get; set; } = new();
    }
} 