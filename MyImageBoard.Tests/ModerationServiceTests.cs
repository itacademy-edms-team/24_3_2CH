using Microsoft.EntityFrameworkCore;
using NewImageBoard.Models;
using NewImageBoard.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NewImageBoard.Tests
{
    public class ModerationServiceTests
    {
        private readonly ImageBoardContext _context;
        private readonly ModerationService _service;

        public ModerationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ImageBoardContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ImageBoardContext(options);
            _service = new ModerationService(_context);

            // Подготовка тестовых данных
            var group = new Group { GroupId = 1, Name = "TestGroup" }; // Добавляем группу
            var user = new User { UserId = 1, Username = "TestUser", GroupId = 1, CreatedAt = DateTime.UtcNow };
            var board = new Board { BoardId = 1, ShortName = "b", Name = "Test Board", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var thread = new ForumThread
            {
                ThreadId = 1,
                BoardId = 1,
                Title = "Test Thread",
                Message = "This is a test thread",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                IsReported = true,
                ImagePath = "image.jpg"
            };
            var post1 = new Post
            {
                PostId = 1,
                ThreadId = 1,
                Message = "This is a test post",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                IsReported = true,
                ImagePath = "post_image.jpg"
            };
            var post2 = new Post
            {
                PostId = 2,
                ThreadId = 1,
                Message = "Another post",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                IsReported = false
            };

            _context.Groups.Add(group); // Добавляем группу для пользователя
            _context.Users.Add(user);
            _context.Boards.Add(board);
            _context.Threads.Add(thread);
            _context.Posts.AddRange(post1, post2);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetFilteredThreadsAsync_ReturnsThreads_WhenSearchTextMatches()
        {
            var filter = new ThreadFilter { SearchText = "test" };
            var result = await _service.GetFilteredThreadsAsync(filter);
            Assert.Single(result);
            Assert.Equal("Test Thread", result.First().Title);
        }

        [Fact]
        public async Task GetFilteredThreadsAsync_ReturnsReportedThreads_WhenIsReportedTrue()
        {
            var filter = new ThreadFilter { IsReported = true };
            var result = await _service.GetFilteredThreadsAsync(filter);
            Assert.Single(result);
            Assert.True(result.First().IsReported);
        }

        [Fact]
        public async Task GetFilteredThreadsAsync_ReturnsThreadsByBoard()
        {
            var filter = new ThreadFilter { BoardId = 1 };
            var result = await _service.GetFilteredThreadsAsync(filter);
            Assert.Single(result);
            Assert.Equal(1, result.First().BoardId);
        }

        [Fact]
        public async Task GetFilteredThreadsAsync_ReturnsThreadsWithImage()
        {
            var filter = new ThreadFilter { HasImage = true };
            var result = await _service.GetFilteredThreadsAsync(filter);
            Assert.Single(result);
            Assert.Equal("image.jpg", result.First().ImagePath);
        }

        [Fact]
        public async Task GetFilteredThreadsAsync_SortsByPostCountDesc()
        {
            _context.Threads.Add(new ForumThread
            {
                ThreadId = 2,
                BoardId = 1,
                Title = "Empty Thread",
                Message = "No posts",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var filter = new ThreadFilter { SortBy = "PostCountDesc" };
            var result = await _service.GetFilteredThreadsAsync(filter);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result.First().ThreadId); // Тред с постами первый
        }

        [Fact]
        public async Task GetFilteredPostsAsync_ReturnsPosts_WhenSearchTextMatches()
        {
            var filter = new PostFilter { SearchText = "test" };
            var result = await _service.GetFilteredPostsAsync(filter);
            Assert.Single(result);
            Assert.Equal("This is a test post", result.First().Message);
        }

        [Fact]
        public async Task GetFilteredPostsAsync_ReturnsReportedPosts_WhenIsReportedTrue()
        {
            var filter = new PostFilter { IsReported = true };
            var result = await _service.GetFilteredPostsAsync(filter);
            Assert.Single(result);
            Assert.True(result.First().IsReported);
        }

        [Fact]
        public async Task GetFilteredPostsAsync_ReturnsPostsByBoard()
        {
            var filter = new PostFilter { BoardId = 1 };
            var result = await _service.GetFilteredPostsAsync(filter);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetFilteredPostsAsync_SortsByMessageAsc()
        {
            var filter = new PostFilter { SortBy = "MessageAsc" };
            var result = await _service.GetFilteredPostsAsync(filter);
            Assert.Equal(2, result.Count);
            Assert.Equal("Another post", result.First().Message);
        }

        [Fact]
        public async Task ClearThreadReportAsync_ClearsReport_WhenThreadExists()
        {
            var success = await _service.ClearThreadReportAsync(1);
            Assert.True(success);
            var thread = await _context.Threads.FindAsync(1);
            Assert.False(thread.IsReported);
        }

        [Fact]
        public async Task DeletePostAsync_DeletesPost_WhenPostExists()
        {
            var success = await _service.DeletePostAsync(1);
            Assert.True(success);
            var post = await _context.Posts.FindAsync(1);
            Assert.Null(post);
        }
    }
}