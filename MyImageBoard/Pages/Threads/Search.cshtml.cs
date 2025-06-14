using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForumProject.Pages.Threads
{
    public class SearchModel : PageModel
    {
        private readonly IThreadService _threadService;
        public SearchModel(IThreadService threadService)
        {
            _threadService = threadService;
        }

        [BindProperty(SupportsGet = true)]
        public ThreadSearchFilter Filter { get; set; } = new ThreadSearchFilter();
        public List<ThreadSearchResult> Results { get; set; }

        public async Task OnGetAsync()
        {
            Results = await _threadService.SearchThreadsAsync(Filter);
        }

        public class ThreadSearchFilter
        {
            public DateTime? CreatedFrom { get; set; }
            public DateTime? CreatedTo { get; set; }
            public int? CommentsFrom { get; set; }
            public int? CommentsTo { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string Tripcode { get; set; }
        }
        public class ThreadSearchResult
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public DateTime CreatedAt { get; set; }
            public int CommentsCount { get; set; }
            public string Tripcode { get; set; }
        }
    }
} 