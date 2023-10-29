
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;
using PiPlayer.Extensions;
using PiPlayer.Models.Common;
using PiPlayer.Models.Common.JsonObject;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Models.ViewModels.Response.FileUpload;
using PiPlayer.Models.ViewModels.Videos;
using PiPlayer.Repository.Interface;
using PiPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PiPlayer.Controllers
{
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;

        public VideoController(ILogger<VideoController> logger
            , ConfigManager config
            , IMediaRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<VideoController>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _repository = repository ?? throw new ArgumentNullException(nameof(IMediaRepository));
        }

        public async Task<IActionResult> Index()
        {
            VideoPageViewModel vm = new VideoPageViewModel();
            List<Media> materials = await _repository.Where(m => m.FileType == FileType.Video).ToListAsync();
            if (materials == null || materials.Count == 0)
            {
                return View(vm);
            }
            foreach (var item in materials)
            {
                item.LogoUrl = UrlPath.Combine(Request, item.LogoUrl);
            }
            vm.Medium = materials;
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Play(long id)
        {
            if (id <= 0)
            {
                return Content("文件ID不能为空。");
            }
            Media item = await _repository.GetAsync(id);
            if (item == null)
            {
                return Json(new ResultModel<IChild>(10033, "查找文件信息失败。"));
            }
            item.LogoUrl = UrlPath.Combine(Request, item.LogoUrl);
            item.FileUrl = UrlPath.Combine(Request, item.Path);
            string fileName = Path.GetFileName(item.FileOldName);
            if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            VideoPlayViewModel vm = new VideoPlayViewModel()
            {
                VideoFileInfo = new FileInfoResult()
                {
                    Id = item.Id,
                    FileName = item.FileName,
                    FileOldName = item.FileOldName,
                    FileSource = item.FileUrl,
                    Path = item.Path,
                    LogoPath = item.LogoUrl,
                    Duration = item.Duration,
                    Extension = item.Extension,
                    FileType = item.FileType,
                    Size = item.Size,
                    Remark = item.Remark,
                    CreatedTime = item.CreatedTime,
                    ContentType = contentType
                }
            };
            return View(vm);
        }
    }
}
