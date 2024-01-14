using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace PiPlayer.Services
{
    public class PlayingService : IPlayingService
    {
        private readonly ILogger<PlayingService> _logger;
        private readonly ConfigManager _config;
        private readonly IMpvService _mpv;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly static object _playLocker = new object();

        public PlayingService(ILogger<PlayingService> logger
            , ConfigManager config
            , IMpvService mpv
            , IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<PlayingService>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(IWebHostEnvironment));
            _mpv = mpv ?? throw new ArgumentNullException(nameof(IMpvService));
        }

        private Task _playingTask = null;
        private CancellationTokenSource _playingCancellationTokenSource = null;
        private long _playingDuration = 0;

        public async Task<(bool, string)> Play(Media media)
        {
            try
            {
                await Stop();
                StartPlayingTask(GenerateCommandItem(media));
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
                await Stop();
                StartPlayingTask(GenerateCommandItem(medium));
                return (true, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Play items failed. {ex.Message}");
                return (false, ex.Message);
            }
        }

        public Task Stop()
        {
            if (_playingTask == null)
            {
                return Task.CompletedTask;
            }
            lock (_playLocker)
            {
                _logger.LogInformation("Waiting for the previous playback...");

                _playingCancellationTokenSource.Cancel();

                bool exitStatus = _playingTask.Wait(TimeSpan.FromMilliseconds(3000));
                if (!exitStatus)
                {
                    throw new TimeoutException("停止播放失败，无法终止当前正在播放的内容。");
                }

                _playingTask.Dispose();
                _playingCancellationTokenSource.Dispose();

                _playingTask = null;
                _playingCancellationTokenSource = null;
            }
            return Task.CompletedTask;
        }

        private List<CommandItem> GenerateCommandItem(Media media)
        {
            return GenerateCommandItem(new List<Media>() { media });
        }

        private List<CommandItem> GenerateCommandItem(IEnumerable<Media> medium)
        {
            List<CommandItem> commandItems = new List<CommandItem>()
            {
                GeneratePlayCommand(medium)
            };
            return commandItems;
        }

        private CommandItem GeneratePlayCommand(IEnumerable<Media> medium)
        {
            CommandItem commandItem = new CommandItem(medium);

            StringBuilder argsBuilder = new StringBuilder();
            foreach (var item in medium)
            {
                argsBuilder.AppendWithSpace($"\"{item.Path}\"");
            }
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

        private void StartPlayingTask(List<CommandItem> playingItems)
        {
            lock (_playLocker)
            {
                _playingCancellationTokenSource = new CancellationTokenSource();
                _playingTask = new Task(() => PlayingTask(playingItems, _playingCancellationTokenSource.Token), TaskCreationOptions.LongRunning);
                _playingTask.Start();
            }
        }

        private async void PlayingTask(IEnumerable<CommandItem> items, CancellationToken cancellationToken)
        {
            if (items?.Any() != true)
            {
                return;
            }

            string setEnvCommand = $"export DISPLAY=:0";

            await Cli.Wrap("/bin/bash")
                .WithArguments(setEnvCommand + "\nexit\n")
                .WithWorkingDirectory((_mpv as BaseFFPlayService).GetBinaryFolder())
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(cancellationToken);

            bool isFinished = false;
            while (!isFinished && !cancellationToken.IsCancellationRequested)
            {
                foreach (var item in items)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            isFinished = true;
                            break;
                        }
                        _logger.LogInformation($"Start play {string.Join(',', item.Medium.Select(p => p.FileOldName))}.");

                        var result = item.Command.ExecuteAsync(cancellationToken)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                        if (result.ExitCode != 0)
                        {
                            _logger.LogInformation($"Play {string.Join(',', item.Medium.Select(p => p.FileOldName))} completed. excute result is {result.ExitCode}");
                            Delay(1000, cancellationToken);
                        }
                        else
                        {
                            _logger.LogInformation($"Play {string.Join(',', item.Medium.Select(p => p.FileOldName))} completed.");
                        }
                        _playingDuration += (int)result.RunTime.TotalSeconds;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation($"Play {string.Join(',', item.Medium.Select(p => p.FileOldName))} completed. closed by operation canceled");
                        isFinished = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Play error, {ex.Message}");
                        //手动实现同步等待
                        Delay(1000, cancellationToken);
                    }
                }
            }
            foreach (CommandItem item in items)
            {
                item.DeletePlaylist();
            }
            _logger.LogInformation($"Playing task exit.");
        }

        private void Delay(int milliseconds, CancellationToken cancellationToken)
        {
            //手动实现同步等待
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                while (!cancellationToken.IsCancellationRequested || stopwatch.ElapsedMilliseconds > milliseconds)
                {
                    Thread.Sleep(10);
                    if (stopwatch.ElapsedMilliseconds > milliseconds)
                    {
                        break;
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
