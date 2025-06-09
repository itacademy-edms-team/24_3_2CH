using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding; // Добавь этот using!

namespace ForumProject.Data.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        public string? Tripcode { get; set; } // Поле для хранения трипкода в формате username#hashedPassword

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool HasComplaint { get; set; } = false;

        [Required]
        public int ThreadId { get; set; }
        [BindNever] 
        public SiteThread Thread { get; set; } = null!;

        // Связь для вложенных комментариев
        public int? ParentCommentId { get; set; } // FK на родительский комментарий (nullable)
        [BindNever] 
        public Comment? ParentComment { get; set; } // Навигационное свойство к родительскому
        [BindNever] 
        public ICollection<Comment> ChildComments { get; set; } = new List<Comment>();


        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}