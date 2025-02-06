using System.Text.Json;
using Moq;
using StackExchange.Redis;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories;

namespace GameOfLife.Tests.Repositories
{
    public class GameOfLifeRepositoryTests
    {
        private readonly Mock<IDatabase> _databaseMock;
        private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
        private readonly GameOfLifeRepository _repository;

        public GameOfLifeRepositoryTests()
        {
            _databaseMock = new Mock<IDatabase>();
            _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
            _connectionMultiplexerMock.Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                                      .Returns(_databaseMock.Object);

            _repository = new GameOfLifeRepository(_connectionMultiplexerMock.Object);
        }

        [Fact]
        public async Task SaveBoard_ShouldSerializeAndStoreBoardInRedis()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var board = new GameOfLifeBoard(new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            })
            {
                Id = boardId
            };

            // Precompute the serialized JSON
            var expectedJson = JsonSerializer.Serialize(board);
            var expectedRedisValue = (RedisValue)expectedJson;

            // Act
            await _repository.SaveBoard(board);

            // Assert
            _databaseMock.Verify(db => db.StringSetAsync(
                boardId.ToString(),
                It.IsAny<RedisValue>(),
                null,
                false,
                When.Always,
                CommandFlags.None),
                Times.Once);
        }

        [Fact]
        public async Task GetBoard_ShouldReturnBoard_WhenExistsInRedis()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var board = new GameOfLifeBoard(new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            })
            {
                Id = boardId
            };

            var boardJson = JsonSerializer.Serialize(board);
            _databaseMock.Setup(db => db.StringGetAsync(boardId.ToString(), CommandFlags.None))
                         .ReturnsAsync(boardJson);

            // Act
            var result = await _repository.GetBoard(boardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(boardId, result?.Id);
            Assert.Equal(board.Board, result?.Board);
        }

        [Fact]
        public async Task GetBoard_ShouldReturnNull_WhenNotExistsInRedis()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            _databaseMock.Setup(db => db.StringGetAsync(boardId.ToString(), CommandFlags.None))
                         .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await _repository.GetBoard(boardId);

            // Assert
            Assert.Null(result);
        }
    }
}
