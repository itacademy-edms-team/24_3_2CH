using System;
using System.Collections.Generic;

namespace MyImageBoard;

public partial class Comment
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Tread> Treads { get; set; } = new List<Tread>();
}
