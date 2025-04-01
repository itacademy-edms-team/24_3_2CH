using System;
using System.Collections.Generic;

namespace MyImageBoard;

public partial class Image
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Tread> Treads { get; set; } = new List<Tread>();
}
