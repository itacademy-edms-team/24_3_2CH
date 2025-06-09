using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class RoleGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Например, "ThreadModerators", "GlobalModerators"

        // Навигационное свойство для связи "многие ко многим" с Role
        public ICollection<RoleGroupRole> RoleGroupRoles { get; set; } = new List<RoleGroupRole>();
        // Навигационное свойство для связи "многие ко многим" с Moderator
        public ICollection<ModeratorRoleGroup> ModeratorRoleGroups { get; set; } = new List<ModeratorRoleGroup>();
    }
}