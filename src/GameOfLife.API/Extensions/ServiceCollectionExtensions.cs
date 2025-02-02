using GameOfLife.API.Services.Interfaces;
using GameOfLife.API.Services;
using StackExchange.Redis;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Repositories;

namespace GameOfLife.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            AddDatabases(services);
            AddServices(services);
            AddRepositories(services);
        }

        private static void AddDatabases(IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddSingleton<IGameOfLifeRepository, GameOfLifeRepository>();
        }
    }
}
