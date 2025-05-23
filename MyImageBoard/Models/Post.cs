using System;
using System.Collections.Generic;

namespace MyImageBoard.Models;

public partial class Post
{
    public int PostId { get; set; }

    public int ThreadId { get; set; }

    public string Message { get; set; } = null!;

    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsReported { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ForumThread Thread { get; set; } = null!;
}
