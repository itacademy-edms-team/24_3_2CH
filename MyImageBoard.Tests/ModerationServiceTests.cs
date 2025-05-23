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
            var logger = new Mock<ILogger<ModerationService>>();
            _service = new ModerationService(_context, logger.Object);

            var board = new Board { BoardId = 1, Name = "Test Board", Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow, ShortName = "b" };
            var thread = new ForumThread { ThreadId = 1, BoardId = 1, Title = "Thread", Message = "Msg", CreatedAt = DateTime.UtcNow, IsReported = true };
            var post = new Post { PostId = 1, ThreadId = 1, Message = "Post", CreatedAt = DateTime.UtcNow, IsReported = true };
            _context.Boards.Add(board);
            _context.Threads.Add(thread);
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetReportedThreadsAsync_ReturnsReportedThreads()
        {
            var threads = await _service.GetReportedThreadsAsync();
            Assert.Single(threads);
            Assert.True(threads.First().IsReported);
        }

        [Fact]
        public async Task GetReportedPostsAsync_ReturnsReportedPosts()
        {
            var posts = await _service.GetReportedPostsAsync();
            Assert.Single(posts);
            Assert.True(posts.First().IsReported);
        }

        [Fact]
        public async Task ClearThreadReportAsync_ClearsReport()
        {
            await _service.ClearThreadReportAsync(1);
            var thread = await _context.Threads.FindAsync(1);
            Assert.False(thread.IsReported);
        }

        [Fact]
        public async Task ClearPostReportAsync_ClearsReport()
        {
            await _service.ClearPostReportAsync(1);
            var post = await _context.Posts.FindAsync(1);
            Assert.False(post.IsReported);
        }

        [Fact]
        public async Task DeleteThreadAsync_DeletesThreadAndPosts()
        {
            await _service.DeleteThreadAsync(1);
            Assert.Empty(_context.Threads);
            Assert.Empty(_context.Posts);
        }

        [Fact]
        public async Task DeletePostAsync_DeletesPost()
        {
            await _service.DeletePostAsync(1);
            Assert.Empty(_context.Posts);
        }
    }
} 