using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly ApplicationDbContext _context;

        public ComplaintService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
        {
            // Проверяем, не существует ли уже жалоба от этого пользователя
            var existingComplaint = await _context.Complaints
                .FirstOrDefaultAsync(c => 
                    c.FingerprintId == complaint.FingerprintId &&
                    ((c.ThreadId == complaint.ThreadId && complaint.ThreadId != null) ||
                     (c.CommentId == complaint.CommentId && complaint.CommentId != null)));

            if (existingComplaint != null)
            {
                throw new InvalidOperationException("Вы уже отправляли жалобу на этот контент.");
            }

            complaint.CreatedAt = DateTime.UtcNow;
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<bool> HasUserComplainedAsync(int fingerprintId, int? threadId, int? commentId)
        {
            return await _context.Complaints
                .AnyAsync(c => 
                    c.FingerprintId == fingerprintId &&
                    ((c.ThreadId == threadId && threadId != null) ||
                     (c.CommentId == commentId && commentId != null)));
        }

        public async Task<IEnumerable<Complaint>> GetComplaintsForThreadAsync(int threadId)
        {
            return await _context.Complaints
                .Include(c => c.Fingerprint)
                .Where(c => c.ThreadId == threadId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Complaint>> GetComplaintsForCommentAsync(int commentId)
        {
            return await _context.Complaints
                .Include(c => c.Fingerprint)
                .Where(c => c.CommentId == commentId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
} 