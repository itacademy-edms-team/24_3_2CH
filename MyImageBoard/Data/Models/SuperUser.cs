using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class SuperUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public bool IsBlocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public int SuperUserGroupId { get; set; }
        public SuperUserGroup Group { get; set; } = null!;
        
        public ICollection<SuperUserPermission> SuperUserPermissions { get; set; } = new List<SuperUserPermission>();
    }
} 