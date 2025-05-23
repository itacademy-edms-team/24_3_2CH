namespace MyImageBoard.Models
{
    public class ThreadFilter
    {
        public int? ThreadId { get; set; }
        public int? BoardId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Author { get; set; }
        public bool? IsReported { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
} 