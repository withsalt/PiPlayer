using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;
using PiPlayer.Services.Base;
using PiPlayer.Utils;

namespace PiPlayer.Services
{
    public class PiPlayerHostService : IHostedService
    {
        private readonly IdleBus<IFreeSql> _ib;
        private readonly ConfigManager _config;
        private readonly ILogger<PiPlayerHostService> _logger;
        private readonly IPlayingService _playService;
        private readonly IConnectionService _connectionService;

        public PiPlayerHostService(ILogger<PiPlayerHostService> logger
            , IdleBus<IFreeSql> ib
            , ConfigManager config
            , IPlayingService playService
            , IConnectionService connectionService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(ILogger<PiPlayerHostService>));
            this._ib = ib;
            this._config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            this._playService = playService ?? throw new ArgumentNullException(nameof(IPlayingService));
            this._connectionService = connectionService ?? throw new ArgumentNullException(nameof(IConnectionService));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await NetworkStatusCheck(cancellationToken);

            if(_config.AppSettings.NetworkCheck && _config.AppSettings.BemfaIot.IsEnabled)
            {
                await _connectionService.ConnectAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _playService.StopPlaying();
            await _connectionService.DisconnectAsync();

            if (_ib != null)
            {
                _ib.Dispose();
            }
        }

        private async Task NetworkStatusCheck(CancellationToken cancellationToken)
        {
            if (!_config.AppSettings.NetworkCheck)
            {
                return;
            }
            try
            {
                bool hasNetwork = false;
                while (!hasNetwork && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var ipAddress = NetworkUtil.GetCurrentGatewayAddresses();
                        if (ipAddress?.Any() == true)
                        {
                            foreach (var ip in ipAddress)
                            {
                                if (await NetworkUtil.PingAsync(ip, cancellationToken))
                                {
                                    hasNetwork = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "启动时校验网络过程中发生错误。");
                    }
                    finally
                    {
                        if (!hasNetwork)
                        {
                            _logger.LogInformation("当前暂无网络连接，等待网络连接恢复...");
                            await Task.Delay(10000, cancellationToken);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message}");
            }
        }
    }
}
