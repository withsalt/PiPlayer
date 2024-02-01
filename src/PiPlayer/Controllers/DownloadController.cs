using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;
using PiPlayer.Models.Entities;
using PiPlayer.Repository.Interface;
using PiPlayer.Services.Base;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PiPlayer.Controllers
{
    public class DownloadController : Controller
    {
        private readonly ILogger<DownloadController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;
        private readonly IDefaultScreenService _defaultScreen;

        public DownloadController(ILogger<DownloadController> logger
            , ConfigManager config
            , IMediaRepository repository
            , IDefaultScreenService defaultScreen)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<DownloadController>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _repository = repository ?? throw new ArgumentNullException(nameof(IMediaRepository));
            _defaultScreen = defaultScreen ?? throw new ArgumentNullException(nameof(IDefaultScreenService));
        }

        public async Task<IActionResult> Index(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "文件ID不能为空。"
                    };
                }
                Media material = await _repository.FindAsync(id);
                if (material == null)
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "获取文件信息失败。"
                    };
                }
                string path = Path.Combine(_config.AppSettings.MediaDirectory, material.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!System.IO.File.Exists(path))
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "当前文件不存在。"
                    };
                }
                string fileName = Path.GetFileName(path);
                if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType))
                {
                    contentType = "application/octet-stream";
                }

                using FileStream fs = new FileStream(path, FileMode.Open);
                return File(fs, contentType, material.FileOldName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Download file failed. {ex.Message}");
                return new ContentResult()
                {
                    StatusCode = 404,
                    Content = $"获取文件失败，错误：{ex.Message}"
                };
            }
        }

        [HttpGet]
        public async Task<IActionResult> DefaultScreen()
        {
            using Image<Rgba32> image = _defaultScreen.GetDefaultScreenImage();
            using MemoryStream ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            return File(ms.ToArray(), "image/png");
        }
    }
}
