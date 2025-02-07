using GameOfLife.API.Services.Interfaces;
using GameOfLife.API.Services;
using StackExchange.Redis;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Repositories;
using GameOfLife.API.Configurations;

namespace GameOfLife.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string GameOfLifeSettings = nameof(GameOfLifeSettings);

        public static void RegisterServices(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            AddConfigurationSettings(services, configurationManager);
            AddDatabases(services, configurationManager);
            AddServices(services);
            AddRepositories(services);
        }

        private static void AddConfigurationSettings(IServiceCollection services, ConfigurationManager configurationManager)
        {
            services.Configure<GameOfLifeSettings>(configurationManager.GetSection(GameOfLifeSettings));
        }

        private static void AddDatabases(IServiceCollection services, ConfigurationManager configurationManager)
        {
            var redisHost = configurationManager["Redis:Host"]!;
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisHost));
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IGameOfLifeService, GameOfLifeService>();
            services.AddSingleton<IGameOfLifeComputeService, GameOfLifeComputeService>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddSingleton<IGameOfLifeRepository, GameOfLifeRepository>();
        }
    }
}
