using ForumProject.Data.Models;
using Microsoft.AspNetCore.Http;

namespace ForumProject.Services.Interfaces
{
    public interface IThreadService
    {
        Task<SiteThread?> GetThreadByIdAsync(int id);
        Task<IEnumerable<SiteThread>> GetAllThreadsAsync();
        Task<(SiteThread? thread, string? errorMessage)> CreateThreadAsync(SiteThread thread, IFormFileCollection? files = null);
        Task<bool> UpdateThreadAsync(SiteThread thread);
        Task<bool> DeleteThreadAsync(int id); // Hard delete
        Task<bool> AddViewsCountAsync(int id); // Для увеличения счетчика просмотров
        Task<List<ForumProject.Pages.Threads.SearchModel.ThreadSearchResult>> SearchThreadsAsync(ForumProject.Pages.Threads.SearchModel.ThreadSearchFilter filter);
        // Добавим методы для обработки жалоб и медиа, когда дойдем до них
    }
}