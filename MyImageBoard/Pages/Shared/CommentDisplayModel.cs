namespace ForumProject.Pages.Shared
{
    public class CommentDisplayModel
    {
        public ForumProject.Data.Models.Comment Comment { get; set; }
        public List<ForumProject.Data.Models.LikeType> AllLikeTypes { get; set; }
        public Dictionary<int, bool> UserReactions { get; set; }
        public Dictionary<int, int> ReactionCounts { get; set; }
        public bool HasUserComplained { get; set; }
        public int UserFingerprintId { get; set; }
        public Dictionary<int, bool> HasUserComplainedComments { get; set; }
        public Dictionary<int, Dictionary<int, bool>> UserCommentReactionsGlobal { get; set; }
        public Dictionary<int, Dictionary<int, int>> CommentReactionCountsGlobal { get; set; }
    }
} 