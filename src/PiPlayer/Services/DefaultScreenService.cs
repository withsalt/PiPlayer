﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using PiPlayer.AspNetCore.FFMpeg;
using PiPlayer.AspNetCore.FFMpeg.Interface;
using PiPlayer.Configs;
using PiPlayer.Extensions;
using PiPlayer.Models.Common;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Services.Base;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PiPlayer.Services
{
    public class DefaultScreenService : BasePlayingService, IDefaultScreenService
    {
        private readonly ILogger<DefaultScreenService> _logger;
        private readonly ConfigManager _config;
        private readonly IServer _server;
        private readonly IMpvService _mpv;

        public DefaultScreenService(ILogger<DefaultScreenService> logger
            , ConfigManager config
            , IServer server
            , IMpvService mpv) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<PlayingService>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _server = server ?? throw new ArgumentNullException(nameof(IServer));
            _mpv = mpv ?? throw new ArgumentNullException(nameof(IMpvService));
        }

        public async Task Show(bool isRefresh = false)
        {
            if (!_config.AppSettings.ShowDefaultScreen)
            {
                return;
            }
            //等10s钟再显示，等网络准备好
            if (!isRefresh)
            {
                await Task.Delay(10000);
            }
            List<CommandItem> cmds = GetCommandItems();
            if (cmds?.Any() != true)
            {
                return;
            }
            await StopPlaying();
            StartPlaying(cmds);
        }

        public Image<Rgba32> GetDefaultScreenImage()
        {
            //构建文本信息
            string showText = BuildInfoText();
            Image<Rgba32> buildImage = CreateImageGrid(TextToImage(showText, 600, 600));
            return buildImage;
        }

        private List<CommandItem> GetCommandItems()
        {
            List<string> endpoints = GetEndpoint();
            if (endpoints?.Any() != true)
            {
                _logger.LogWarning("获取本地监听网络地址失败！");
                return null;
            }

            List<Media> medias = new List<Media>()
            {
                new Media()
                {
                    FileName = "DefaultScreen.png",
                    FileOldName = "DefaultScreen.png",
                    Path = endpoints.OrderByDescending(p=>p).First() + "/Download/DefaultScreen",
                    FileType = FileType.Image
                }
            };

            CommandItem commandItem = new CommandItem(medias);
            StringBuilder argsBuilder = new StringBuilder();
            argsBuilder.AppendWithSpace($"\"{medias[0].Path}\"");
            //指定播放屏幕
            argsBuilder.AppendWithSpace($"--screen={_config.AppSettings.Screen.Index}");

            argsBuilder.AppendWithSpace("--loop-playlist=inf");
            argsBuilder.AppendWithSpace("--no-border");
            //全屏播放禁用控制器
            argsBuilder.AppendWithSpace("--osc=no");
            //隐藏光标
            argsBuilder.AppendWithSpace("--cursor-autohide=always");

            //即使没有视频，也创建一个视频输出窗口。
            argsBuilder.AppendWithSpace("--force-window=yes");
            argsBuilder.AppendWithSpace("--ontop");

            //禁用默认的输入绑定
            argsBuilder.AppendWithSpace("--no-input-default-bindings");
            //禁用窗口自动调整
            argsBuilder.AppendWithSpace("--no-keepaspect-window");
            argsBuilder.AppendWithSpace("--keep-open");
            //如果当前文件是图像，则播放图像指定的秒数（默认值：1）
            argsBuilder.AppendWithSpace($"--image-display-duration=10");
            //固定窗口和填充大小
            if (_config.AppSettings.Screen.FullScreen)
            {
                argsBuilder.AppendWithSpace("--fullscreen");
            }
            else
            {
                argsBuilder.AppendWithSpace($"--geometry={_config.AppSettings.Screen.Width}x{_config.AppSettings.Screen.Height}");
                argsBuilder.AppendWithSpace($"--autofit={_config.AppSettings.Screen.Width}x{_config.AppSettings.Screen.Height}");
            }

            commandItem.Command = Cli.Wrap(_mpv.GetBinaryPath())
                    .WithArguments(argsBuilder.ToString())
                    .WithWorkingDirectory((_mpv as BaseFFPlayService).GetBinaryFolder())
                    .WithValidation(CommandResultValidation.None);
            return new List<CommandItem>()
            {
                commandItem
            };
        }

        private string BuildInfoText()
        {
            //获取WIFI信息
            StringBuilder showText = new StringBuilder();
            if (_config.AppSettings.IsEnableAP)
            {
                showText.AppendLine("WIFI:");
                showText.AppendLine("    " + _config.AppSettings.APName + " / " + _config.AppSettings.APPasswd);
            }
            showText.AppendLine();
            //获取本地访问地址
            List<string> endpoints = GetEndpoint();
            if (endpoints?.Any() == true)
            {
                showText.AppendLine("WebSite:");
                if (endpoints?.Any() != true)
                {
                    showText.AppendLine("    --");
                }
                else
                {
                    foreach (var item in endpoints)
                    {
                        showText.AppendLine("    " + item);
                    }
                }
            }
            return showText.ToString();
        }

        private Image<Rgba32> CreateImageGrid(Image<Rgba32> baseImage)
        {
            // 假设所有的图片都有相同的尺寸
            int imageWidth = baseImage.Width;
            int imageHeight = baseImage.Height;
            //搞一个9宫格出来
            Image<Rgba32>[] images = new Image<Rgba32>[9];
            try
            {
                Image<Rgba32> blackImage = new Image<Rgba32>(imageWidth, imageHeight);
                blackImage.Mutate(x => x.Fill(Color.Black));
                images[0] = blackImage;


                images[1] = baseImage;

                images[2] = blackImage.Clone();

                var image3 = baseImage.Clone();
                image3.Mutate(x => x.Rotate(90));
                images[3] = image3;

                images[4] = blackImage.Clone();

                var image5 = baseImage.Clone();
                image5.Mutate(x => x.Rotate(270));
                images[5] = image5;

                images[6] = blackImage.Clone();

                var image7 = baseImage.Clone();
                image7.Mutate(x => x.Rotate(180));
                images[7] = image7;
                images[8] = blackImage.Clone();

                // 创建一个新的图片，宽度和高度是单个图片的3倍
                Image<Rgba32> outputImage = new Image<Rgba32>(imageWidth * 3, imageHeight * 3);
                // 在新的图片上绘制每一张图片
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int imageIndex = i * 3 + j;
                        outputImage.Mutate(ctx => ctx.DrawImage(images[imageIndex], new Point(imageWidth * j, imageHeight * i), 1));
                    }
                }

                // 返回新的图片
                return outputImage;
            }
            finally
            {
                if (images?.Any() == true)
                {
                    foreach (var image in images)
                    {
                        image.Dispose();
                    }
                }
            }
        }

        private Image<Rgba32> TextToImage(string text, int width, int height)
        {
            Image<Rgba32> image = new Image<Rgba32>(width, height);

            FontCollection collection = new();
            FontFamily family = collection.Add("./Fonts/SourceHanSerifCN-Medium-6.otf");
            Font font = family.CreateFont(40, FontStyle.Bold);

            image.Mutate(x => x
                .Fill(Color.Black)
                .DrawText(text, font, Color.Red, new PointF(30, 100)));
            return image;
        }

        /// <summary>
        /// 获取监听地址
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<string> GetEndpoint()
        {
            var address = _server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (address == null || address.Length == 0)
            {
                throw new Exception("Can not get current app endpoint.");
            }
            List<string> urls = new List<string>();
            var pattern = @"^(?<scheme>https?):\/\/((\+)|(\*)|\[::\]|(0.0.0.0))(?=[\:\/]|$)";
            foreach (var endpoint in address)
            {
                Match match = Regex.Match(endpoint, pattern);
                if (!match.Success)
                {
                    continue;
                }
                var localIpaddress = GetLocalNetworkAddress();
                if (localIpaddress?.Any() != true)
                {
                    continue;
                }
                foreach (var ipItem in localIpaddress)
                {
                    var uri = Regex.Replace(endpoint, @"^(?<scheme>https?):\/\/((\+)|(\*)|\[::\]|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://" + ipItem);
                    Uri httpEndpoint = new Uri(uri, UriKind.Absolute);
                    urls.Add(new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port).ToString().TrimEnd('/'));
                }
            }
            return urls;
        }

        /// <summary>
        /// 获取本机地址
        /// </summary>
        /// <returns></returns>
        private List<string> GetLocalNetworkAddress()
        {
            try
            {
                // 获取本地计算机上的所有网络接口
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    ?.Where(p => p.Supports(NetworkInterfaceComponent.IPv4) && p.OperationalStatus == OperationalStatus.Up)
                    .ToArray();
                if (networkInterfaces?.Any() != true)
                    return new List<string>();
                List<UnicastIPAddressInformation> allIps = new List<UnicastIPAddressInformation>();
                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    // 获取IPv6地址信息
                    var ipAddresses = networkInterface.GetIPProperties()?.UnicastAddresses?
                        .Where(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !p.Address.ToString().StartsWith("127"))
                        .ToArray();
                    if (ipAddresses?.Any() != true) continue;
                    allIps.AddRange(ipAddresses);
                }

                if (!allIps.Any())
                {
                    return new List<string>();
                }
                return allIps.Select(p => p.Address.ToString())
                    .OrderBy(p => p)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取本机IP地址失败，错误：{ex.Message}");
            }
            return null;
        }
    }
}