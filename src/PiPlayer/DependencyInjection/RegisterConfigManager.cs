using Microsoft.Extensions.DependencyInjection;
using PiPlayer.Configs;

namespace PiPlayer.DependencyInjection
{
    public static class RegisterConfigManager
    {
        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            services.AddSingleton<ConfigManager>();

            using (var provider = services.BuildServiceProvider())
            {
                ConfigManager config = provider.GetRequiredService<ConfigManager>();
                config.Load();
            }

            return services;
        }
    }
}
