using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class Moderator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)] // Для хранения соли
        public string Salt { get; set; } = string.Empty;

        // Навигационное свойство для связи "многие ко многим" с RoleGroup
        public ICollection<ModeratorRoleGroup> ModeratorRoleGroups { get; set; } = new List<ModeratorRoleGroup>();
    }
}