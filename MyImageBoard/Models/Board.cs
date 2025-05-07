using System;
using System.Collections.Generic;

namespace NewImageBoard.Models;

public partial class Board
{
    public int BoardId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public string ShortName { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<ForumThread> Threads { get; set; } = new List<ForumThread>();
}
