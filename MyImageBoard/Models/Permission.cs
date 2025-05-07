using System;
using System.Collections.Generic;

namespace NewImageBoard.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
