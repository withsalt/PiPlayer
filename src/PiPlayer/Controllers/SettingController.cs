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
    public class SettingController : Controller
    {
        private readonly ILogger<SettingController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;
        private readonly IPlayingService _playService;
        private readonly IMpvService _mpv;
        private readonly IFFMpegService _ffmpeg;
        private readonly IDefaultScreenService _defaultScreen;

        public SettingController(ILogger<SettingController> logger
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

        public IActionResult Index()
        {
            return View();
        }

    }
}
