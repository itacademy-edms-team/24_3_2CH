using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class SuperUserPermission
    {
        [Key]
        public int Id { get; set; }

        public int SuperUserId { get; set; }
        public SuperUser SuperUser { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }
} 