using Microsoft.EntityFrameworkCore;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;

namespace MyImageBoard.Services;

public class PostService : IPostService
{
    private readonly ImageBoardContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<PostService> _logger;
    private readonly string[] _allowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    private readonly int _maxImageSizeInBytes = 5 * 1024 * 1024; // 5MB

    public PostService(
        ImageBoardContext context,
        IUserService userService,
        ILogger<PostService> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Post> GetPostByIdAsync(int id)
    {
        try
        {
            var post = await _context.Posts
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Thread)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post is null)
            {
                throw new KeyNotFoundException($"Post with ID {id} not found");
            }

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post by ID {PostId}", id);
            throw;
        }
    }

    public async Task<Post> CreatePostAsync(Post post, int userId)
    {
        try
        {
            // Разрешаем анонимам создавать посты
            if (userId > 0 && !await _userService.HasPermissionAsync(userId, "CreatePost"))
            {
                throw new UnauthorizedAccessException("User does not have permission to create posts");
            }

            if (!await ValidatePostContentAsync(post.Message))
            {
                throw new InvalidOperationException("Post content is invalid");
            }

            post.CreatedBy = userId > 0 ? userId : null;
            post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            throw;
        }
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        try
        {
            var existingPost = await _context.Posts.FindAsync(post.PostId);
            if (existingPost is null)
            {
                throw new KeyNotFoundException($"Post with ID {post.PostId} not found");
            }

            if (!await IsPostOwnerAsync(post.PostId, post.CreatedBy!.Value) &&
                !await CanUserModeratePostAsync(post.PostId, post.CreatedBy!.Value))
            {
                throw new UnauthorizedAccessException("User does not have permission to modify this post");
            }

            if (!await ValidatePostContentAsync(post.Message))
            {
                throw new InvalidOperationException("Post content is invalid");
            }

            existingPost.Message = post.Message;
            existingPost.ImagePath = post.ImagePath;

            await _context.SaveChangesAsync();

            return existingPost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", post.PostId);
            throw;
        }
    }

    public async Task DeletePostAsync(int id, int userId)
    {
        try
        {
            var post = await _context.Posts.FindAsync(id);
            if (post is null)
            {
                throw new KeyNotFoundException($"Post with ID {id} not found");
            }

            if (!await IsPostOwnerAsync(id, userId) &&
                !await CanUserModeratePostAsync(id, userId))
            {
                throw new UnauthorizedAccessException("User does not have permission to delete this post");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            throw;
        }
    }

    public async Task<bool> IsPostOwnerAsync(int postId, int userId)
    {
        try
        {
            var post = await _context.Posts.FindAsync(postId);
            return post?.CreatedBy == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is owner of post {PostId}", userId, postId);
            throw;
        }
    }

    public async Task<bool> CanUserModeratePostAsync(int postId, int userId)
    {
        try
        {
            var post = await _context.Posts
                .Include(p => p.Thread)
                .ThenInclude(t => t.Board)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post is null)
            {
                return false;
            }

            return await _userService.HasPermissionAsync(userId, "ModeratePost") ||
                   await _userService.HasPermissionAsync(userId, "Admin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can moderate post {PostId}", userId, postId);
            throw;
        }
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 10)
    {
        try
        {
            return await _context.Posts
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.Thread)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent posts");
            throw;
        }
    }

    public Task<bool> ValidatePostContentAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Task.FromResult(false);
        }

        // Проверка на минимальную и максимальную длину
        if (content.Length < 1 || content.Length > 10000)
        {
            return Task.FromResult(false);
        }

        // Проверка на запрещенные слова (можно добавить список)
        var forbiddenWords = new[] { "spam", "scam", "hack" };
        if (forbiddenWords.Any(word => content.Contains(word, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<bool> ValidateImageAsync(IFormFile image)
    {
        if (image is null)
        {
            return Task.FromResult(true); // Изображение необязательно
        }

        // Проверка расширения файла
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_allowedImageExtensions.Contains(extension))
        {
            return Task.FromResult(false);
        }

        // Проверка размера файла
        if (image.Length > _maxImageSizeInBytes)
        {
            return Task.FromResult(false);
        }

        // Проверка MIME-типа
        if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
} 