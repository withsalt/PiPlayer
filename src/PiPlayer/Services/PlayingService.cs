using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PiPlayer.AspNetCore.FFMpeg;
using PiPlayer.AspNetCore.FFMpeg.Interface;
using PiPlayer.Configs;
using PiPlayer.Extensions;
using PiPlayer.Models.Common;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Services.Base;

namespace PiPlayer.Services
{
    public class PlayingService : BasePlayingService, IPlayingService
    {
        private readonly ILogger<PlayingService> _logger;
        private readonly ConfigManager _config;
        private readonly IMpvService _mpv;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PlayingService(ILogger<PlayingService> logger
            , ConfigManager config
            , IMpvService mpv
            , IWebHostEnvironment hostingEnvironment) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<PlayingService>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            _mpv = mpv ?? throw new ArgumentNullException(nameof(IMpvService));
        }

        public async Task<(bool, string)> Play(Media media)
        {
            try
            {
                await StopPlaying();
                StartPlaying(GenerateCommandItem(media));
                return (true, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Play item failed. {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> PlayQueue(IEnumerable<Media> medium)
        {
            try
            {
                await StopPlaying();
                StartPlaying(GenerateCommandItem(medium));
                return (true, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Play items failed. {ex.Message}");
                return (false, ex.Message);
            }
        }

        private List<CommandItem> GenerateCommandItem(Media media)
        {
            return GenerateCommandItem(new List<Media>() { media });
        }

        private CommandItem GeneratePlayCommand(IEnumerable<Media> medium)
        {
            CommandItem commandItem = new CommandItem(medium);

            StringBuilder argsBuilder = new StringBuilder();
            foreach (var item in medium)
            {
                argsBuilder.AppendWithSpace($"\"{item.Path}\"");
            }
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
            if (medium.Any(p => p.FileType == FileType.Image))
                argsBuilder.AppendWithSpace($"--image-display-duration={_config.AppSettings.ImagePlaybackInterval}");
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
            return commandItem;
        }


        private List<CommandItem> GenerateCommandItem(IEnumerable<Media> medium)
        {
            List<CommandItem> commandItems = new List<CommandItem>()
            {
                GeneratePlayCommand(medium)
            };
            return commandItems;
        }
    }
}
