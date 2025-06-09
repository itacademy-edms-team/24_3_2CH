using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = null!;

        // Navigation properties
        public ICollection<SuperUserPermission> SuperUserPermissions { get; set; } = new List<SuperUserPermission>();
        public ICollection<SuperUserGroupPermission> GroupPermissions { get; set; } = new List<SuperUserGroupPermission>();
    }
} 