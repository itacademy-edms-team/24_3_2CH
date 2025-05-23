using MyImageBoard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyImageBoard.Services.Interfaces
{
    public interface IModerationService
    {
        Task<IEnumerable<ForumThread>> GetReportedThreadsAsync();
        Task<IEnumerable<Post>> GetReportedPostsAsync();
        Task ClearThreadReportAsync(int threadId);
        Task ClearPostReportAsync(int postId);
        Task DeleteThreadAsync(int threadId);
        Task DeletePostAsync(int postId);
        Task ReportThreadAsync(int threadId);

        Task<IEnumerable<ForumThread>> FindThreadsAsync(ThreadFilter filter);
        Task<IEnumerable<Post>> FindPostsAsync(PostFilter filter);
    }
} 