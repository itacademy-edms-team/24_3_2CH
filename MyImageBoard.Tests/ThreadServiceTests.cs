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
    public class ThreadServiceTests
    {
        private readonly ImageBoardContext _context;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<ThreadService>> _mockLogger;
        private readonly ThreadService _service;

        public ThreadServiceTests()
        {
            var options = new DbContextOptionsBuilder<ImageBoardContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ImageBoardContext(options);
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<ThreadService>>();
            _service = new ThreadService(_context, _mockUserService.Object, _mockLogger.Object);

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
        public async Task GetThreadsByBoardAsync_ReturnsThreadsForBoard()
        {
            // Arrange
            var boardId = 1;
            var page = 1;
            var pageSize = 10;

            // Act
            var result = await _service.GetThreadsByBoardAsync(boardId, page, pageSize);

            // Assert
            Assert.Single(result);
            var thread = result.First();
            Assert.Equal("Test Thread", thread.Title);
            Assert.Equal("Test Message", thread.Message);
        }

        [Fact]
        public async Task GetThreadByIdAsync_ReturnsCorrectThread()
        {
            // Arrange
            var threadId = 1;

            // Act
            var result = await _service.GetThreadByIdAsync(threadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(threadId, result.ThreadId);
            Assert.Equal("Test Thread", result.Title);
        }

        [Fact]
        public async Task GetThreadByIdAsync_ThrowsKeyNotFoundException_WhenThreadNotFound()
        {
            // Arrange
            var threadId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetThreadByIdAsync(threadId));
        }

        [Fact]
        public async Task CreateThreadAsync_CreatesNewThread_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "CreateThread"))
                .ReturnsAsync(true);

            var newThread = new ForumThread
            {
                BoardId = 1,
                Title = "New Thread",
                Message = "New Message"
            };

            // Act
            var result = await _service.CreateThreadAsync(newThread, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Thread", result.Title);
            Assert.Equal("New Message", result.Message);
            Assert.Equal(1, result.CreatedBy);
            Assert.True(result.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task CreateThreadAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "CreateThread"))
                .ReturnsAsync(false);

            var newThread = new ForumThread
            {
                BoardId = 1,
                Title = "New Thread",
                Message = "New Message"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.CreateThreadAsync(newThread, 1));
        }

        [Fact]
        public async Task UpdateThreadAsync_UpdatesThread_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModifyThread"))
                .ReturnsAsync(true);

            var thread = new ForumThread
            {
                ThreadId = 1,
                BoardId = 1,
                Title = "Updated Thread",
                Message = "Updated Message",
                CreatedBy = 1
            };

            // Act
            var result = await _service.UpdateThreadAsync(thread);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Thread", result.Title);
            Assert.Equal("Updated Message", result.Message);
        }

        [Fact]
        public async Task UpdateThreadAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModifyThread"))
                .ReturnsAsync(false);

            var thread = new ForumThread
            {
                ThreadId = 1,
                BoardId = 1,
                Title = "Updated Thread",
                Message = "Updated Message",
                CreatedBy = 2
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.UpdateThreadAsync(thread));
        }

        [Fact]
        public async Task DeleteThreadAsync_DeletesThread_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "DeleteThread"))
                .ReturnsAsync(true);

            // Act
            await _service.DeleteThreadAsync(1, 1);

            // Assert
            var thread = await _context.Threads.FindAsync(1);
            Assert.Null(thread);
        }

        [Fact]
        public async Task DeleteThreadAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "DeleteThread"))
                .ReturnsAsync(false);

            // Мокаем методы сервиса
            var threadServiceMock = new Mock<IThreadService>();
            threadServiceMock.Setup(x => x.IsThreadOwnerAsync(1, 1)).ReturnsAsync(false);
            threadServiceMock.Setup(x => x.CanUserModerateThreadAsync(1, 1)).ReturnsAsync(false);

            // Меняем владельца треда на 2
            var thread = await _context.Threads.FindAsync(1);
            thread.CreatedBy = 2;
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.DeleteThreadAsync(1, 1));
        }

        [Fact]
        public async Task IsThreadOwnerAsync_ReturnsTrue_WhenUserIsOwner()
        {
            // Arrange
            var threadId = 1;
            var userId = 1;

            // Act
            var result = await _service.IsThreadOwnerAsync(threadId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanUserModerateThreadAsync_ReturnsTrue_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModerateThread"))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanUserModerateThreadAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetThreadPostsAsync_ReturnsPostsForThread()
        {
            // Arrange
            var threadId = 1;
            var page = 1;
            var pageSize = 10;

            // Act
            var result = await _service.GetThreadPostsAsync(threadId, page, pageSize);

            // Assert
            Assert.Single(result);
            var post = result.First();
            Assert.Equal("Test Post", post.Message);
        }

        [Fact]
        public async Task GetThreadPostsCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var threadId = 1;

            // Act
            var result = await _service.GetThreadPostsCountAsync(threadId);

            // Assert
            Assert.Equal(1, result);
        }
    }
} 