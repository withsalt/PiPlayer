using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Web;
using OnceMi.AspNetCore.IdGenerator;
using PiPlayer.AspNetCore.FFMpeg;
using PiPlayer.Configs;
using PiPlayer.DependencyInjection;
using PiPlayer.Services;
using PiPlayer.Services.Base;

namespace PiPlayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //设置当前目录为程序集所在目录
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddConfig();
            builder.Logging.ClearProviders();
            builder.Logging.AddNLogWeb();

            builder.Services.AddFFmpegService();
            builder.Services.AddSingleton<IPlayingService, PlayingService>();
            builder.Services.AddSingleton<IDefaultScreenService, DefaultScreenService>();

            builder.Services.AddSingleton<IConnectionService, ConnectionService>();

            //缓存
            builder.Services.AddMemoryCache();
            //数据库
            builder.Services.AddIdGenerator(x =>
            {
                x.AppId = 1;
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(GlobalConfigConstant.DefaultOriginsName, policy =>
                {
                    policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
                });
            });

            //使用Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
            });

            //Db
            builder.Services.AddDatabase();
            builder.Services.AddRepository();

            builder.Services.AddMapper();

            builder.Services.AddHostedService<PiPlayerHostService>();

            // Add services to the container.
            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            builder.Services.Configure<FormOptions>(options =>
            {
                //修改上传限制100GB
                options.MultipartBodyLengthLimit = 107374182400;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //初始化数据库
            app.InitializeDatabase();

            //跨域
            app.UseCors(GlobalConfigConstant.DefaultOriginsName);

            app.UseStaticFiles();
            app.UseMediaStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Video}/{action=Index}/{id?}");

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                IDefaultScreenService defaultScreen = app.Services.GetRequiredService<IDefaultScreenService>();
                defaultScreen.Show();
            });

            try
            {
                app.Run();
            }
            catch (TaskCanceledException) { }
        }
    }
}