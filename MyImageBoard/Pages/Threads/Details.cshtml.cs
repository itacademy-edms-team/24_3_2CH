using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ModelBinding; //  ModelState.Remove

namespace ForumProject.Pages.Threads
{
    public class DetailsModel : PageModel
    {
        private readonly IThreadService _threadService;
        private readonly ICommentService _commentService;
        private readonly ILikeService _likeService; 
        private readonly ILikeTypeService _likeTypeService;
        private readonly IUserFingerprintService _userFingerprintService; 
        private readonly IComplaintService _complaintService;
        private readonly IMediaFileService _mediaFileService;

        public DetailsModel(
            IThreadService threadService,
            ICommentService commentService,
            ILikeService likeService,
            ILikeTypeService likeTypeService,
            IUserFingerprintService userFingerprintService,
            IComplaintService complaintService,
            IMediaFileService mediaFileService)
        {
            _threadService = threadService;
            _commentService = commentService;
            _likeService = likeService;
            _likeTypeService = likeTypeService;
            _userFingerprintService = userFingerprintService;
            _complaintService = complaintService;
            _mediaFileService = mediaFileService;
        }

        public SiteThread? Thread { get; set; }

        [BindProperty]
        public Comment NewComment { get; set; } = new Comment();

        [BindProperty]
        public Complaint NewComplaint { get; set; } = new Complaint();

        public int CurrentUserFingerprintId { get; set; }

        public List<LikeType> AllLikeTypes { get; set; } = new List<LikeType>();

        // Информация о реакциях пользователя на тред
        public Dictionary<int, bool> UserThreadReactions { get; set; } = new Dictionary<int, bool>();

        // Количество реакций каждого типа на тред
        public Dictionary<int, int> ThreadReactionCounts { get; set; } = new Dictionary<int, int>();

        // Информация о реакциях пользователя на комментарии
        public Dictionary<int, Dictionary<int, bool>> UserCommentReactions { get; set; } = new Dictionary<int, Dictionary<int, bool>>();

        // Количество реакций каждого типа на комментарии
        public Dictionary<int, Dictionary<int, int>> CommentReactionCounts { get; set; } = new Dictionary<int, Dictionary<int, int>>();

        public bool HasCurrentUserComplainedThread { get; set; }

        public Dictionary<int, bool> HasCurrentUserComplainedComments { get; set; } = new Dictionary<int, bool>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Thread = await _threadService.GetThreadByIdAsync(id);

            if (Thread == null)
            {
                ViewData["BoardId"] = TempData["BoardId"]; // Сохраняем BoardId для возврата на доску
                return Page(); // Страница сама отрендерит сообщение о том, что тред не найден
            }

            NewComment.ThreadId = Thread.Id;

            var userFingerprint = await _userFingerprintService.GetOrCreateFingerprintAsync(HttpContext);
            CurrentUserFingerprintId = userFingerprint.Id;

            // Получаем все типы реакций
            AllLikeTypes = await _likeTypeService.GetAllActiveLikeTypesAsync();

            // Получаем информацию о реакциях пользователя на тред
            UserThreadReactions = await _likeService.GetUserReactionsForThreadAsync(CurrentUserFingerprintId, Thread.Id);

            // Получаем количество реакций каждого типа на тред
            ThreadReactionCounts = await _likeService.GetReactionCountsForThreadAsync(Thread.Id);

            // Получаем все комментарии (включая вложенные)
            var allComments = GetAllCommentsFlat(Thread.Comments ?? new List<Comment>());

            // Получаем информацию о реакциях пользователя на комментарии
            UserCommentReactions = await _likeService.GetUserReactionsForCommentsAsync(CurrentUserFingerprintId, allComments);

            // Получаем количество реакций каждого типа на комментарии
            CommentReactionCounts = await _likeService.GetReactionCountsForCommentsAsync(allComments);

            HasCurrentUserComplainedThread = await _complaintService.HasUserComplainedAsync(CurrentUserFingerprintId, Thread.Id, null);

            // Рекурсивно обрабатываем все комментарии для проверки жалоб
            if (Thread.Comments != null)
            {
                await ProcessComplaintsRecursivelyAsync(Thread.Comments);
            }

            ViewData["BoardId"] = Thread.BoardId;
            TempData["BoardId"] = Thread.BoardId;

            return Page();
        }

        private List<Comment> GetAllCommentsFlat(ICollection<Comment> comments)
        {
            var all = new List<Comment>();
            foreach (var comment in comments)
            {
                all.Add(comment);
                if (comment.ChildComments != null && comment.ChildComments.Any())
                    all.AddRange(GetAllCommentsFlat(comment.ChildComments));
            }
            return all;
        }

        private async Task ProcessComplaintsRecursivelyAsync(ICollection<Comment> comments)
        {
            foreach (var comment in comments)
            {
                HasCurrentUserComplainedComments[comment.Id] = await _complaintService.HasUserComplainedAsync(CurrentUserFingerprintId, null, comment.Id);

                // Рекурсивно обрабатываем дочерние комментарии
                if (comment.ChildComments != null && comment.ChildComments.Any())
                {
                    await ProcessComplaintsRecursivelyAsync(comment.ChildComments);
                }
            }
        }

        public async Task<IActionResult> OnPostAddCommentAsync()
        {
            try
            {
                // Проверяем файлы, если они есть
                var files = Request.Form.Files;
                if (files != null && files.Count > 0)
                {
                    var validationResult = await _mediaFileService.ValidateFilesAsync(files);
                    if (!validationResult.isValid)
                    {
                        TempData["ErrorMessage"] = validationResult.errorMessage;
                        return RedirectToPage("./Details", new { id = NewComment.ThreadId });
                    }
                }

                // Создаем комментарий
                var comment = await _commentService.CreateCommentAsync(NewComment);

                // Если есть файлы, сохраняем их
                if (files != null && files.Count > 0)
                {
                    await _mediaFileService.SaveCommentFilesAsync(files, comment.Id, NewComment.ThreadId);
                }

                TempData["SuccessMessage"] = "Комментарий успешно добавлен";
                return RedirectToPage("./Details", new { id = NewComment.ThreadId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при создании комментария: {ex.Message}";
                return RedirectToPage("./Details", new { id = NewComment.ThreadId });
            }
        }

        //    
        public async Task<IActionResult> OnPostToggleLikeAsync(int? threadId, int? commentId, int likeTypeId)
        {
            if (!threadId.HasValue && !commentId.HasValue)
            {
                return BadRequest("Не указан ID треда или комментария для лайка.");
            }

            var userFingerprint = await _userFingerprintService.GetOrCreateFingerprintAsync(HttpContext);
            CurrentUserFingerprintId = userFingerprint.Id;

            await _likeService.ToggleLikeAsync(CurrentUserFingerprintId, threadId, commentId, likeTypeId);

            int redirectThreadId = threadId.HasValue ? threadId.Value : Thread?.Id ?? 0;
            if (redirectThreadId == 0)
            {
                if (commentId.HasValue)
                {
                    var comment = await _commentService.GetCommentByIdAsync(commentId.Value);
                    if (comment != null) redirectThreadId = comment.ThreadId;
                }
            }

            return RedirectToPage("./Details", new { id = redirectThreadId });
        }

        public async Task<IActionResult> OnPostAddComplaintAsync(int? threadId, int? commentId)
        {
            if (!threadId.HasValue && !commentId.HasValue)
            {
                return BadRequest("Необходимо указать ID треда или комментария.");
            }

            // Если жалоба на комментарий, получаем threadId из комментария
            int targetThreadId = threadId ?? (await _commentService.GetCommentByIdAsync(commentId!.Value))!.ThreadId;

            var userFingerprint = await _userFingerprintService.GetOrCreateFingerprintAsync(HttpContext);
            NewComplaint.FingerprintId = userFingerprint.Id;
            NewComplaint.ThreadId = threadId;
            NewComplaint.CommentId = commentId;

            try
            {
                await _complaintService.CreateComplaintAsync(NewComplaint);
                TempData["SuccessMessage"] = "Жалоба успешно отправлена.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("./Details", new { id = targetThreadId });
        }
    }
}