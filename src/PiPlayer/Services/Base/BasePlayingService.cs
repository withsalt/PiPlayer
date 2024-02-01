using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PiPlayer.Models.Common;

namespace PiPlayer.Services.Base
{
    public class BasePlayingService
    {
        private readonly ILogger _logger;
        private Task _playingTask = null;
        private CancellationTokenSource _playingCancellationTokenSource = null;
        private long _playingDuration = 0;
        private readonly static object _playLocker = new object();

        public BasePlayingService(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            StopPlaying();
        }

        public Task StopPlaying()
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

        protected void StartPlaying(List<CommandItem> playingItems)
        {
            lock (_playLocker)
            {
                _playingCancellationTokenSource = new CancellationTokenSource();
                _playingTask = new Task(() => PlayingTask(playingItems, _playingCancellationTokenSource.Token), TaskCreationOptions.LongRunning);
                _playingTask.Start();
            }
        }

        private void PlayingTask(IEnumerable<CommandItem> items, CancellationToken cancellationToken)
        {
            if (items?.Any() != true)
            {
                return;
            }

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
                            .ConfigureAwait(false).GetAwaiter().GetResult();
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

    }
}
