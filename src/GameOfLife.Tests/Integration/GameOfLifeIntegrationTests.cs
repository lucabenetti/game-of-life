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

namespace GameOfLife.Tests.Integration
{
    public class GameOfLifeIntegrationTests : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer;
        private HttpClient? _client;
        private WebApplicationFactory<Program>? _factory;
        private IConnectionMultiplexer? _redisConnection;

        public GameOfLifeIntegrationTests()
        {
            _redisContainer = new RedisBuilder().WithImage("redis:7").Build();
        }

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
            _redisConnection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());

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

        [Fact]
        public async Task GetNextState_Returns_Computed_State()
        {
            var board = new int[][]
            {
            new int[] { 0, 1, 0 },
            new int[] { 1, 0, 1 },
            new int[] { 0, 1, 0 }
            };

            var uploadResponse = await _client.PostAsJsonAsync("api/gameoflife/upload", board);
            uploadResponse.EnsureSuccessStatusCode();
            var apiResponse = await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var boardId = apiResponse.Data;

            var nextStateResponse = await _client.GetAsync($"api/gameoflife/{boardId}/next");
            nextStateResponse.EnsureSuccessStatusCode();
            var nextState = await nextStateResponse.Content.ReadFromJsonAsync<ApiResponse<int[][]>>();

            Assert.NotNull(nextState);
            Assert.True(nextState.Success);
            Assert.NotNull(nextState.Data);
        }

        [Fact]
        public async Task GetFinalState_Returns_Correct_Termination()
        {
            var board = new int[][]
            {
            new int[] { 0, 1, 0 },
            new int[] { 1, 0, 1 },
            new int[] { 0, 1, 0 }
            };

            var uploadResponse = await _client.PostAsJsonAsync("api/gameoflife/upload", board);
            uploadResponse.EnsureSuccessStatusCode();
            var apiResponse = await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            var boardId = apiResponse.Data;

            var finalStateResponse = await _client.GetAsync($"api/gameoflife/{boardId}/final/10");
            finalStateResponse.EnsureSuccessStatusCode();
            var finalState = await finalStateResponse.Content.ReadFromJsonAsync<ApiResponse<FinalStateResultDto>>();

            Assert.NotNull(finalState);
            Assert.True(finalState.Success);
            Assert.NotNull(finalState.Data);
            Assert.True(finalState.Data.Completed);
        }
    }
}
