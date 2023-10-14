using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Models.ViewModels.Videos;
using PiPlayer.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PiPlayer.Controllers
{
    public class AudioController : Controller
    {
        private readonly ILogger<AudioController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;

        public AudioController(ILogger<AudioController> logger
            , ConfigManager config
            , IMediaRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<AudioController>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _repository = repository ?? throw new ArgumentNullException(nameof(IMediaRepository));
        }

        public async Task<IActionResult> Index()
        {
            VideoPageViewModel vm = new VideoPageViewModel();
            List<Media> materials = await _repository.Where(m => m.FileType == FileType.Music).ToListAsync();
            if (materials == null || materials.Count == 0)
            {
                return View(vm);
            }
            string hostUrl = Request.Host.Value;

            string baseHost = $"{Request.Scheme}://{Request.Host}/";
            foreach (var item in materials)
            {
                item.LogoUrl = baseHost + item.LogoUrl;
            }
            vm.Medium = materials;
            return View(vm);
        }
    }
}
