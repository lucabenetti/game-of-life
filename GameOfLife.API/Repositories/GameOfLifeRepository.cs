using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace GameOfLife.API.Repositories
{
    public class GameOfLifeRepository : IGameOfLifeRepository
    {
        private readonly IDatabase _database;

        public GameOfLifeRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task SaveBoard(GameOfLifeBoard board)
        {
            await _database.StringSetAsync(board.Id.ToString(), JsonSerializer.Serialize(board));
        }

        public async Task<GameOfLifeBoard?> GetBoard(Guid id)
        {
            var boardJson = await _database.StringGetAsync(id.ToString());
            return boardJson.IsNullOrEmpty ? null : JsonSerializer.Deserialize<GameOfLifeBoard>(boardJson!);
        }
    }
}
