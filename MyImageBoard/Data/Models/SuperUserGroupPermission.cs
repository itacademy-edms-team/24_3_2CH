using System.ComponentModel.DataAnnotations;

namespace ForumProject.Data.Models
{
    public class SuperUserGroupPermission
    {
        [Key]
        public int Id { get; set; }

        public int SuperUserGroupId { get; set; }
        public SuperUserGroup SuperUserGroup { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
} 