namespace MyImageBoard.Models
{
    public class PostFilter
    {
        public int? PostId { get; set; }
        public int? ThreadId { get; set; }
        public string Message { get; set; }
        public string Author { get; set; }
        public bool? IsReported { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 