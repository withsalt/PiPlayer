using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PiPlayer.Configs;
using System.IO;

namespace PiPlayer.DependencyInjection
{
    public static class RegisterMediaStaticFiles
    {
        public static IApplicationBuilder UseMediaStaticFiles(this WebApplication app)
        {
            var config = app.Services.GetService<ConfigManager>();
            if (!Directory.Exists(config.AppSettings.MediaDirectory))
            {
                Directory.CreateDirectory(config.AppSettings.MediaDirectory);
            }
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(config.AppSettings.MediaDirectory),
                RequestPath = new PathString("/" + GlobalConfigConstant.DefaultMediaDirectory),
            });
            return app;
        }
    }
}
