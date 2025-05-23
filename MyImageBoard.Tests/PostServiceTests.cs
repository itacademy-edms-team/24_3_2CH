using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyImageBoard.Models;
using MyImageBoard.Services;
using MyImageBoard.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyImageBoard.Tests
{
    public class PostServiceTests
    {
        private readonly ImageBoardContext _context;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<PostService>> _mockLogger;
        private readonly PostService _service;

        public PostServiceTests()
        {
            var options = new DbContextOptionsBuilder<ImageBoardContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ImageBoardContext(options);
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<PostService>>();
            _service = new PostService(_context, _mockUserService.Object, _mockLogger.Object);

            // Подготовка тестовых данных
            var group = new Group { GroupId = 1, Name = "TestGroup" };
            var user = new User { UserId = 1, Username = "TestUser", GroupId = 1, CreatedAt = DateTime.UtcNow, PasswordHash = "testhash" };
            var board = new Board 
            { 
                BoardId = 1, 
                ShortName = "b", 
                Name = "Test Board", 
                Description = "Test Description",
                CreatedBy = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            var thread = new ForumThread
            {
                ThreadId = 1,
                BoardId = 1,
                Title = "Test Thread",
                Message = "Test Message",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                IsReported = false
            };
            var post = new Post
            {
                PostId = 1,
                ThreadId = 1,
                Message = "Test Post",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                IsReported = false
            };

            _context.Groups.Add(group);
            _context.Users.Add(user);
            _context.Boards.Add(board);
            _context.Threads.Add(thread);
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetPostByIdAsync_ReturnsCorrectPost()
        {
            // Arrange
            var postId = 1;

            // Act
            var result = await _service.GetPostByIdAsync(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.PostId);
            Assert.Equal("Test Post", result.Message);
        }

        [Fact]
        public async Task GetPostByIdAsync_ThrowsKeyNotFoundException_WhenPostNotFound()
        {
            // Arrange
            var postId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetPostByIdAsync(postId));
        }

        [Fact]
        public async Task CreatePostAsync_CreatesNewPost_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "CreatePost"))
                .ReturnsAsync(true);

            var newPost = new Post
            {
                ThreadId = 1,
                Message = "New Post"
            };

            // Act
            var result = await _service.CreatePostAsync(newPost, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Post", result.Message);
            Assert.Equal(1, result.CreatedBy);
            Assert.True(result.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task CreatePostAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "CreatePost"))
                .ReturnsAsync(false);

            var newPost = new Post
            {
                ThreadId = 1,
                Message = "New Post"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.CreatePostAsync(newPost, 1));
        }

        [Fact]
        public async Task UpdatePostAsync_UpdatesPost_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModifyPost"))
                .ReturnsAsync(true);

            var post = new Post
            {
                PostId = 1,
                ThreadId = 1,
                Message = "Updated Post",
                CreatedBy = 1
            };

            // Act
            var result = await _service.UpdatePostAsync(post);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Post", result.Message);
        }

        [Fact]
        public async Task UpdatePostAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModifyPost"))
                .ReturnsAsync(false);

            var post = new Post
            {
                PostId = 1,
                ThreadId = 1,
                Message = "Updated Post",
                CreatedBy = 2
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.UpdatePostAsync(post));
        }

        [Fact]
        public async Task DeletePostAsync_DeletesPost_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "DeletePost"))
                .ReturnsAsync(true);

            // Act
            await _service.DeletePostAsync(1, 1);

            // Assert
            var post = await _context.Posts.FindAsync(1);
            Assert.Null(post);
        }

        [Fact]
        public async Task DeletePostAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "DeletePost"))
                .ReturnsAsync(false);

            // Мокаем методы сервиса
            var postServiceMock = new Mock<IPostService>();
            postServiceMock.Setup(x => x.IsPostOwnerAsync(1, 1)).ReturnsAsync(false);
            postServiceMock.Setup(x => x.CanUserModeratePostAsync(1, 1)).ReturnsAsync(false);

            // Меняем владельца поста на 2
            var post = await _context.Posts.FindAsync(1);
            post.CreatedBy = 2;
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.DeletePostAsync(1, 1));
        }

        [Fact]
        public async Task IsPostOwnerAsync_ReturnsTrue_WhenUserIsOwner()
        {
            // Arrange
            var postId = 1;
            var userId = 1;

            // Act
            var result = await _service.IsPostOwnerAsync(postId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanUserModeratePostAsync_ReturnsTrue_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModeratePost"))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanUserModeratePostAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetRecentPostsAsync_ReturnsRecentPosts()
        {
            // Arrange
            var count = 10;

            // Act
            var result = await _service.GetRecentPostsAsync(count);

            // Assert
            Assert.Single(result);
            var post = result.First();
            Assert.Equal("Test Post", post.Message);
        }

        [Fact]
        public async Task ValidatePostContentAsync_ReturnsTrue_ForValidContent()
        {
            // Arrange
            var content = "Valid post content";

            // Act
            var result = await _service.ValidatePostContentAsync(content);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidatePostContentAsync_ReturnsFalse_ForInvalidContent()
        {
            // Arrange
            var content = "";

            // Act
            var result = await _service.ValidatePostContentAsync(content);

            // Assert
            Assert.False(result);
        }
    }
} 