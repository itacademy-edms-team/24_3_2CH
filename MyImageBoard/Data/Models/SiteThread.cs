using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    public class SiteThread
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; } // Содержимое треда, включая Markdown и блоки опросов

        public string? Tripcode { get; set; } // Поле для хранения трипкода в формате username#hashedPassword

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool HasComplaint { get; set; } = false;

        public int ViewsCount { get; set; } = 0; // Необязательно, но полезно
        
        
        [Required]
        public int BoardId { get; set; } // FK к Board
        [BindNever]
        public Board Board { get; set; } = null!; // Навигационное свойство к доске

        // Навигационные свойства
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); // Для опросов, если они будут частью треда

        /// <summary>
        /// Теги треда, разделённые запятой (например: "аниме,wallpaper,art")
        /// </summary>
        [MaxLength(256)]
        public string? Tags { get; set; }
    }
}