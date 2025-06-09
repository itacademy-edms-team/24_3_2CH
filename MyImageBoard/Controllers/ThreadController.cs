using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForumProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly IThreadService _threadService;
        private readonly IMediaFileService _mediaFileService;

        public ThreadController(IThreadService threadService, IMediaFileService mediaFileService)
        {
            _threadService = threadService;
            _mediaFileService = mediaFileService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateThread([FromForm] ThreadCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Валидация файлов, если они есть
            if (request.Files != null && request.Files.Any())
            {
                var (isValid, errorMessage) = await _mediaFileService.ValidateFilesAsync(request.Files);
                if (!isValid)
                {
                    return BadRequest(new { error = errorMessage });
                }
            }

            // Создаем тред
            var thread = new SiteThread
            {
                Title = request.Title,
                Content = request.Content,
                BoardId = request.BoardId,
                Tripcode = request.Tripcode
            };

            var (createdThread, error) = await _threadService.CreateThreadAsync(thread, request.Files);
            if (createdThread == null)
            {
                return BadRequest(new { error });
            }

            return Ok(new { 
                threadId = createdThread.Id,
                mediaFiles = createdThread.MediaFiles.Select(f => new {
                    id = f.Id,
                    fileName = f.FileName,
                    fileType = f.FileType,
                    fileSize = f.Size
                })
            });
        }

        [HttpDelete("file/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var file = await _mediaFileService.DeleteFileAsync(fileId.ToString());
            if (!file)
            {
                return NotFound();
            }
            return Ok();
        }
    }

    public class ThreadCreateRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int BoardId { get; set; }
        public string? Tripcode { get; set; }
        public IFormFileCollection? Files { get; set; }
    }
} 