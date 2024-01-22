using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;

namespace PiPlayer.Services
{
    public class BackgroundEventService : IHostedService
    {
        private readonly ILogger<BackgroundEventService> _logger;
        private readonly ConfigManager _config;
        private readonly IPlayingService _playService;

        public BackgroundEventService(ILogger<BackgroundEventService> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IPlayingService playService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<BackgroundEventService>));
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._playService = playService ?? throw new ArgumentNullException(nameof(IPlayingService));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _playService.Stop();

            return Task.CompletedTask;
        }
    }
}
