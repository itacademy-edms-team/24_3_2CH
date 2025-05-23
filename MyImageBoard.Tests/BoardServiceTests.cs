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
    public class BoardServiceTests
    {
        private readonly ImageBoardContext _context;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<BoardService>> _mockLogger;
        private readonly BoardService _service;

        public BoardServiceTests()
        {
            var options = new DbContextOptionsBuilder<ImageBoardContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ImageBoardContext(options);
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<BoardService>>();
            _service = new BoardService(_context, _mockUserService.Object, _mockLogger.Object);

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

            _context.Groups.Add(group);
            _context.Users.Add(user);
            _context.Boards.Add(board);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllBoardsAsync_ReturnsAllBoards()
        {
            // Arrange
            var expectedCount = 1;

            // Act
            var result = await _service.GetAllBoardsAsync();

            // Assert
            Assert.Equal(expectedCount, result.Count());
            var board = result.First();
            Assert.Equal("Test Board", board.Name);
            Assert.Equal("b", board.ShortName);
        }

        [Fact]
        public async Task GetBoardByIdAsync_ReturnsCorrectBoard()
        {
            // Arrange
            var boardId = 1;

            // Act
            var result = await _service.GetBoardByIdAsync(boardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(boardId, result.BoardId);
            Assert.Equal("Test Board", result.Name);
        }

        [Fact]
        public async Task GetBoardByIdAsync_ThrowsKeyNotFoundException_WhenBoardNotFound()
        {
            // Arrange
            var boardId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetBoardByIdAsync(boardId));
        }

        [Fact]
        public async Task GetBoardByShortNameAsync_ReturnsCorrectBoard()
        {
            // Arrange
            var shortName = "b";

            // Act
            var result = await _service.GetBoardByShortNameAsync(shortName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(shortName, result.ShortName);
            Assert.Equal("Test Board", result.Name);
        }

        [Fact]
        public async Task GetBoardByShortNameAsync_ThrowsKeyNotFoundException_WhenBoardNotFound()
        {
            // Arrange
            var shortName = "nonexistent";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetBoardByShortNameAsync(shortName));
        }

        [Fact]
        public async Task CreateBoardAsync_CreatesNewBoard_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "CreateBoard"))
                .ReturnsAsync(true);

            var newBoard = new Board
            {
                Name = "New Board",
                ShortName = "nb",
                Description = "New Description"
            };

            // Act
            var result = await _service.CreateBoardAsync(newBoard, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Board", result.Name);
            Assert.Equal("nb", result.ShortName);
            Assert.Equal(1, result.CreatedBy);
            Assert.True(result.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task CreateBoardAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "CreateBoard"))
                .ReturnsAsync(false);

            var newBoard = new Board
            {
                Name = "New Board",
                ShortName = "nb",
                Description = "New Description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.CreateBoardAsync(newBoard, 1));
        }

        [Fact]
        public async Task UpdateBoardAsync_UpdatesBoard_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModifyBoard"))
                .ReturnsAsync(true);

            var board = new Board
            {
                BoardId = 1,
                Name = "Updated Board",
                ShortName = "ub",
                Description = "Updated Description",
                CreatedBy = 1
            };

            // Act
            var result = await _service.UpdateBoardAsync(board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Board", result.Name);
            Assert.Equal("ub", result.ShortName);
            Assert.Equal("Updated Description", result.Description);
        }

        [Fact]
        public async Task UpdateBoardAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModifyBoard"))
                .ReturnsAsync(false);

            var board = new Board
            {
                BoardId = 1,
                Name = "Updated Board",
                ShortName = "ub",
                Description = "Updated Description",
                CreatedBy = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.UpdateBoardAsync(board));
        }

        [Fact]
        public async Task DeleteBoardAsync_DeletesBoard_WhenUserHasPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "DeleteBoard"))
                .ReturnsAsync(true);

            // Act
            await _service.DeleteBoardAsync(1);

            // Assert
            var board = await _context.Boards.FindAsync(1);
            Assert.Null(board);
        }

        [Fact]
        public async Task DeleteBoardAsync_ThrowsUnauthorizedAccessException_WhenUserHasNoPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "DeleteBoard"))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.DeleteBoardAsync(1));
        }

        [Fact]
        public async Task IsUserModeratorAsync_ReturnsTrue_WhenUserHasModerateBoardPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "ModerateBoard"))
                .ReturnsAsync(true);

            // Act
            var result = await _service.IsUserModeratorAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserAdminAsync_ReturnsTrue_WhenUserHasAdminPermission()
        {
            // Arrange
            _mockUserService.Setup(x => x.HasPermissionAsync(1, "Admin"))
                .ReturnsAsync(true);

            // Act
            var result = await _service.IsUserAdminAsync(1);

            // Assert
            Assert.True(result);
        }
    }
} 