using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace PiPlayer.DependencyInjection
{
    public static class RegisterAutoMapper
    {
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.AddProfile(new UserInfoMapper());
            });
            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            return services;
        }
    }
}
