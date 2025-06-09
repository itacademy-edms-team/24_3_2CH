using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class SuperUserGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<SuperUser> SuperUsers { get; set; } = new List<SuperUser>();
        public ICollection<SuperUserGroupPermission> GroupPermissions { get; set; } = new List<SuperUserGroupPermission>();
    }
} 