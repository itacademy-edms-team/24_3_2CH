using System;
using System.Collections.Generic;

namespace NewImageBoard.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int GroupId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Board> Boards { get; set; } = new List<Board>();

    public virtual Group Group { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<ForumThread> Threads { get; set; } = new List<ForumThread>();
}
