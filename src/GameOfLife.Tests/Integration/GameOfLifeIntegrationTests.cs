using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using GameOfLife.API;
using GameOfLife.API.DTOs;
using Testcontainers.Redis;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Repositories;
using GameOfLife.API.Services.Interfaces;
using GameOfLife.API.Services;
using DotNet.Testcontainers.Builders;

namespace GameOfLife.Tests.Unit.Integration
{
    public class GameOfLifeIntegrationTests : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer;
        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;
        private IConnectionMultiplexer _redisConnection;

        public GameOfLifeIntegrationTests()
        {
            _redisContainer = new RedisBuilder()
                .WithImage("redis:7")
                .WithCleanUp(true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379)) // Ensure Redis is ready for GitHub Actions
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();

            // Retry logic for Redis connection
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    _redisConnection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
                    if (_redisConnection.IsConnected)
                        break;
                }
                catch
                {
                    await Task.Delay(2000); // Wait before retrying
                }
            }

            if (_redisConnection == null || !_redisConnection.IsConnected)
            {
                throw new Exception("Failed to connect to Redis within the timeout.");
            }

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<IConnectionMultiplexer>();
                        services.AddSingleton<IConnectionMultiplexer>(_redisConnection);
                        services.RemoveAll<IGameOfLifeRepository>();
                        services.AddSingleton<IGameOfLifeRepository, GameOfLifeRepository>();
                        services.RemoveAll<IGameOfLifeService>();
                        services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
                    });
                });

            _client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            await _redisContainer.DisposeAsync();
            _factory.Dispose();
            _redisConnection.Dispose();
        }

        [Fact]
        public async Task UploadBoard_Returns_Created_And_Stores_Board()
        {
            var board = new int[][]
            {
            new int[] { 0, 1, 0 },
            new int[] { 1, 0, 1 },
            new int[] { 0, 1, 0 }
            };

            var response = await _client.PostAsJsonAsync("api/gameoflife/upload", board);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotEqual(Guid.Empty, apiResponse.Data);

            var boardKey = apiResponse.Data.ToString();
            var redisValue = await _redisConnection.GetDatabase().StringGetAsync(boardKey);
            Assert.False(redisValue.IsNullOrEmpty);
        }
    }
}
