using ForumProject.Data.Models;

namespace ForumProject.Pages.Shared
{
    public class ReactionsViewModel
    {
        public List<LikeType> AllLikeTypes { get; set; } = new List<LikeType>();
        public Dictionary<int, bool> UserReactions { get; set; } = new Dictionary<int, bool>();
        public Dictionary<int, int> ReactionCounts { get; set; } = new Dictionary<int, int>();
        public int? ThreadId { get; set; }
        public int? CommentId { get; set; }
    }
} 