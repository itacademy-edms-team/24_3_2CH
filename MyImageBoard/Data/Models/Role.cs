using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Например, "CanDeleteThread", "CanRemoveThreadComplaint"

        // Навигационное свойство для связи "многие ко многим" с RoleGroup
        public ICollection<RoleGroupRole> RoleGroupRoles { get; set; } = new List<RoleGroupRole>();
    }
}