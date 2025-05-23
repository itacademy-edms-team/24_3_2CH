using MyImageBoard.Models;
using Microsoft.AspNetCore.Http;

namespace MyImageBoard.Services.Interfaces;

public interface IPostService
{
    Task<Post> GetPostByIdAsync(int id);
    Task<Post> CreatePostAsync(Post post, int userId);
    Task<Post> UpdatePostAsync(Post post);
    Task DeletePostAsync(int id, int userId);
    Task<bool> IsPostOwnerAsync(int postId, int userId);
    Task<bool> CanUserModeratePostAsync(int postId, int userId);
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 10);
    Task<bool> ValidatePostContentAsync(string content);
    Task<bool> ValidateImageAsync(IFormFile image);
} 