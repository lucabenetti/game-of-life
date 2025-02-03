using Serilog;

namespace GameOfLife.API.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.WithProperty("Application", nameof(API))
                .CreateLogger();

            builder.Logging.AddSerilog(logger);

            return builder;
        }
    }
}
