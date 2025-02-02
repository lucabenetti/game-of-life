using GameOfLife.API.Services.Interfaces;
using GameOfLife.API.Services;
using StackExchange.Redis;

namespace GameOfLife.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
            services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
        }
    }
}
