using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Models.ViewModels.Videos;
using PiPlayer.Repository.Interface;
using PiPlayer.Utils;

namespace PiPlayer.Controllers
{
    public class ImagesController : Controller
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;

        public ImagesController(ILogger<ImagesController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMediaRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<ImagesController>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _repository = repository ?? throw new ArgumentNullException(nameof(IMediaRepository));
        }

        public async Task<IActionResult> Index()
        {
            VideoPageViewModel vm = new VideoPageViewModel();
            List<Media> materials = await _repository.Where(m => m.FileType == FileType.Image).ToListAsync();
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
    }
}
