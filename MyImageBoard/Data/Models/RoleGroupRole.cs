using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    // Эта сущность представляет собой соединительную таблицу для связи "многие ко многим"
    // между RoleGroup и Role.
    public class RoleGroupRole
    {
        public int RoleGroupId { get; set; }
        public RoleGroup RoleGroup { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}