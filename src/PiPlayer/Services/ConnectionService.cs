using System;
using System.Threading.Tasks;
using BemfaCloud;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Devices;
using BemfaCloud.Models;
using Microsoft.Extensions.Logging;
using PiPlayer.Configs;
using PiPlayer.Services.Base;

namespace PiPlayer.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly ILogger<ConnectionService> _logger;
        private readonly ConfigManager _config;
        private readonly IPlayingService _playService;
        private IBemfaConnector _connector;
        private BemfaSwitch _switch;

        public ConnectionService(ILogger<ConnectionService> logger
            , ConfigManager config
            , IPlayingService playService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<ConnectionService>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _playService = playService ?? throw new ArgumentNullException(nameof(IPlayingService));
            _connector = CreateConnector();
        }

        private IBemfaConnector CreateConnector()
        {
            if (!_config.AppSettings.BemfaIot.IsEnabled)
            {
                return null;
            }
            IBemfaConnector connector = new BemfaConnectorBuilder()
                .WithTcp()
                .WithSecret(_config.AppSettings.BemfaIot.PrivateKey)
                .WithTopics(_config.AppSettings.BemfaIot.DeviceId)
                .WithErrorHandler((e) =>
                {
                    switch (e.LogType)
                    {
                        case LogType.Error:
                        case LogType.Warining:
                            _logger.LogWarning($"[巴法云]{e.Message}");
                            break;
                        default:
                        case LogType.Info:
                            break;
                    }
                })
                .Build();
            return connector;
        }

        private bool _switchStatus = false;

        public async Task<bool> ConnectAsync()
        {
            ThrowIfDisposed();

            if (_connector == null)
            {
                return true;
            }

            try
            {
                _logger.LogInformation($"[巴法云]正在连接到巴法云，设备Id：{_config.AppSettings.BemfaIot.DeviceId}，协议：{_connector.ProtocolType}...");

                // 创建开关对象
                _switch = new BemfaSwitch(_config.AppSettings.BemfaIot.DeviceId, _connector);

                // 注册开关事件
                _switch.On += (e) =>
                {
                    _switchStatus = true;
                    _playService.PlayRandom();

                    //1.5s之后发送关闭指令，重新关闭
                    if (_switchStatus)
                    {
                        _ = Task.Run(async () =>
                        {
                            if (_switchStatus)
                            {
                                await Task.Delay(1500);
                                await _connector.UpdateAsync(_config.AppSettings.BemfaIot.DeviceId, "off");
                            }
                        });
                    }
                    return true;
                };

                _switch.Off += (e) =>
                {
                    _switchStatus = false;
                    return true;
                };

                _switch.OnException += (e) =>
                {
                    _logger.LogError(e, "[巴法云]巴法云连接发生异常");
                };

                _switch.OnMessage += (e) =>
                {
                    _logger.LogWarning($"[巴法云]收到无法解析的消息：{e.ToString()}");
                };

                // 连接到服务器
                bool isConnect = await _connector.ConnectAsync();
                if (isConnect)
                {
                    _logger.LogInformation("[巴法云]已成功连接到巴法云");
                    return true;
                }
                else
                {
                    _logger.LogWarning("[巴法云]连接到巴法云失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[巴法云]连接巴法云IOT失败");
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            ThrowIfDisposed();

            if (_connector == null)
            {
                return true;
            }

            try
            {
                // 清理开关对象
                if (_switch != null)
                {
                    _switch = null;
                }
                // 断开连接
                _ = await _connector.DisconnectAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[巴法云]断开巴法云IOT连接失败");
                return false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ConnectionService));
            }
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 释放托管资源
                if (_connector != null)
                {
                    _connector.Dispose();
                    _connector = null;
                }
            }

            _disposed = true;
        }
    }
}
