using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<bool> HasUserComplainedAsync(int fingerprintId, int? threadId, int? commentId);
        Task<IEnumerable<Complaint>> GetComplaintsForThreadAsync(int threadId);
        Task<IEnumerable<Complaint>> GetComplaintsForCommentAsync(int commentId);
    }
} 