using System;
using System.Collections.Generic;

namespace NewImageBoard.Models;

public partial class ForumThread
{
    public int ThreadId { get; set; }

    public int BoardId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsReported { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
