using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PiPlayer.AspNetCore.FFMpeg.Interface;
using PiPlayer.AspNetCore.FFMpeg.Models;
using PiPlayer.Configs;
using PiPlayer.Models.Common;
using PiPlayer.Models.Common.JsonObject;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Models.ViewModels.Request;
using PiPlayer.Repository.Interface;
using PiPlayer.Services.Base;

namespace PiPlayer.Controllers
{
    public class ControlController : Controller
    {
        private readonly ILogger<ControlController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;
        private readonly IPlayingService _playService;
        private readonly IMpvService _mpv;
        private readonly IFFMpegService _ffmpeg;
        private readonly IDefaultScreenService _defaultScreen;

        public ControlController(ILogger<ControlController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMediaRepository repository
            , IPlayingService playService
            , IMpvService mpv
            , IFFMpegService ffmpeg
            , IDefaultScreenService defaultScreen)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<ControlController>));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._playService = playService ?? throw new ArgumentNullException(nameof(IPlayingService));
            this._repository = repository ?? throw new ArgumentNullException(nameof(IMediaRepository));
            this._mpv = mpv ?? throw new ArgumentNullException(nameof(IMpvService));
            this._ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(IFFMpegService));
            this._defaultScreen = defaultScreen ?? throw new ArgumentNullException(nameof(IDefaultScreenService));
        }

        [HttpPost]
        public async Task<IActionResult> Play(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new ResultModel<IChild>(10011, "播放文件ID不能为空。"));
                }
                Media material = await _repository.FindAsync(id);
                if (material == null)
                {
                    return Json(new ResultModel<IChild>(10012, "获取文件信息失败。"));
                }
                string filePath = Path.Combine(_config.AppSettings.DataDirectory, material.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new ResultModel<IChild>(10012, "播放的文件不存在。"));
                }
                material.Path = filePath;
                (bool, string) result = (false, null);
                switch (material.FileType)
                {
                    case FileType.Video:
                        result = await _playService.Play(material);
                        break;
                    case FileType.Music:
                        result = await _playService.Play(material);
                        break;
                    case FileType.Image:
                        result = await _playService.Play(material);
                        break;
                    default:
                        return Json(new ResultModel<IChild>(10013, "不支持的文件类型。"));
                }
                if (result.Item1)
                {
                    return Json(new ResultModel<IChild>(0));
                }
                else
                {
                    return Json(new ResultModel<IChild>(10013, result.Item2));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Json(new ResultModel<IChild>(10013, "系统错误，详细信息请查看日志。"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlayQueue(PlayQueueParams @params)
        {
            if (@params == null || @params.Items == null || @params.Items.Count == 0)
            {
                return Json(new ResultModel<IChild>(10021, "播放序列为空。"));
            }
            List<object> playIds = new List<object>();
            foreach (var item in @params.Items)
            {
                if (item.Id == 0)
                {
                    return Json(new ResultModel<IChild>(10021, "播放序列中包含无效的ID。"));
                }
                playIds.Add(item.Id);
            }
            List<Media> materials = await _repository.Where(p => playIds.Contains(p.Id)).ToListAsync();
            if (materials.Count != @params.Items.Count)
            {
                return Json(new ResultModel<IChild>(10022, "获取播放序列信息失败。"));
            }
            List<Media> orderdPlayList = new List<Media>();
            foreach (var item in @params.Items)
            {
                var media = materials.FirstOrDefault(p => p.Id == item.Id);
                if (media == null)
                    throw new FileNotFoundException($"Can not found file by id '{item.Id}'.");
                string filePath = Path.Combine(_config.AppSettings.DataDirectory, media.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new ResultModel<IChild>(10012, $"播放序列中包含不存在的文件，文件名：{media.FileOldName}。"));
                }
                media.Path = filePath;
                orderdPlayList.Add(media);
            }
            (bool, string) result = await _playService.PlayQueue(orderdPlayList);
            if (result.Item1)
            {
                return Json(new ResultModel<IChild>(0));
            }
            else
            {
                return Json(new ResultModel<IChild>(10013, result.Item2));
            }
        }

        [HttpPost]
        public IActionResult Stop()
        {
            _playService.StopPlaying();
            return Json(new ResultModel<IChild>(0));
        }

        [HttpPost]
        public async Task<IActionResult> Status()
        {
            LibVersion mpvVersion = await _mpv.GetVersion();
            LibVersion ffmpegVersion = await _ffmpeg.GetVersion();

            StringBuilder stringBuilder = new StringBuilder();
            if (ffmpegVersion.Status)
            {
                stringBuilder.Append("FFMpeg：");
                stringBuilder.Append(string.IsNullOrEmpty(ffmpegVersion.Version) ? "正常，版本未知" : $"正常（{ffmpegVersion.Version}）");
            }
            else
            {
                stringBuilder.Append("FFMpeg：异常");
            }
            stringBuilder.Append("<br />");
            if (mpvVersion.Status)
            {
                stringBuilder.Append("&ensp;&ensp;&ensp;MPV：");
                stringBuilder.Append(string.IsNullOrEmpty(mpvVersion.Version) ? "正常，版本未知" : $"正常（{mpvVersion.Version}）");
            }
            else
            {
                stringBuilder.Append("&ensp;&ensp;&ensp;MPV：异常");
            }
            return Json(new ResultModel<IChild>(mpvVersion.Status && ffmpegVersion.Status ? 0 : -1, stringBuilder.ToString()));
        }

        [HttpPost]
        public IActionResult Reboot()
        {
            string command = "shutdown";
            string args = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                args = "-r -t 0 -f";
            }
            else
            {
                args = "-r now";
            }

            Task.Run(async () =>
            {
                await Task.Delay(500);
                CommandResult result = await Cli.Wrap(command)
                        .WithArguments(args)
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
            });
            return Json(new ResultModel<IChild>(0));
        }

        [HttpPost]
        public IActionResult Shutdown()
        {
            string command = "shutdown";
            string args = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                args = "-s -t 0 -f";
            }
            else
            {
                args = "-h now";
            }

            Task.Run(async () =>
            {
                await Task.Delay(500);
                CommandResult result = await Cli.Wrap(command)
                        .WithArguments(args)
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
            });
            return Json(new ResultModel<IChild>(0));
        }

        [HttpPost]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                await _defaultScreen.Show(true);
                return Json(new ResultModel<IChild>(0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刷新默认状态显示失败，错误：{ex.Message}");
                return Json(new ResultModel<IChild>(-1, ex.Message));
            }
        }
    }
}
