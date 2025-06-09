using System.ComponentModel.DataAnnotations.Schema;

namespace ForumProject.Data.Models
{
    // Эта сущность представляет собой соединительную таблицу для связи "многие ко многим"
    // между Moderator и RoleGroup.
    public class ModeratorRoleGroup
    {
        public int ModeratorId { get; set; }
        public Moderator Moderator { get; set; } = null!;

        public int RoleGroupId { get; set; }
        public RoleGroup RoleGroup { get; set; } = null!;
    }
}