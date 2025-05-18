using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FreeSql;
using PiPlayer.Models.Enums;
using PiPlayer.Utils;

namespace PiPlayer.Configs.Models
{
    public class AppSettingsNode
    {
        public List<AllowExtensionNode> AllowExtensions { get; set; }

        public BemfaIot BemfaIot { get; set; }

        public ScreenMode Screen { get; set; }


        private int imagePlaybackInterval = 5;

        /// <summary>
        /// 图片播放间隔
        /// </summary>
        public int ImagePlaybackInterval
        {
            set
            {
                imagePlaybackInterval = value;
            }
            get
            {
                if (imagePlaybackInterval < 3) return 3;
                return imagePlaybackInterval;
            }
        }

        private string _dataDirectory = null;

        public string DataDirectory
        {
            get
            {
                return _dataDirectory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(DataDirectory), "Data directory can not null");
                }

                string dic = value;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    dic = dic.Replace('/', Path.DirectorySeparatorChar);
                else
                    dic = dic.Replace('\\', Path.DirectorySeparatorChar);

                if (CommonHelper.TryParseLocalPathString(dic, "%BASE%", AppContext.BaseDirectory, out string connTemp))
                {
                    dic = connTemp;
                }
                _dataDirectory = dic;
            }
        }

        public string MediaDirectory
        {
            get
            {
                return Path.Combine(this.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory);
            }
        }

        public string TmpDirectory
        {
            get
            {
                return Path.Combine(this.DataDirectory, GlobalConfigConstant.DefaultTempDirectory);
            }
        }

        public DefaultScreenNode DefaultScreen { get; set; }

        public Database Database { get; set; }


        /// <summary>
        /// 启动时网络状态检查
        /// </summary>
        public bool NetworkCheck {  get; set; }
    }

    public class ScreenMode
    {
        private int _index = 0;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index < 0) throw new ArgumentOutOfRangeException(nameof(Index));
                _index = value;
            }
        }

        public int Width { get; set; }

        public int Height { get; set; }

        private bool _fullScreen = false;
        public bool FullScreen
        {
            get => _fullScreen;
#if DEBUG
            set => _fullScreen = false;
#else
            set => _fullScreen = value;
#endif
        }
    }

    public class DefaultScreenNode
    {
        public bool IsEnable { get; set; }

        private DefaultScreenContentType _type = DefaultScreenContentType.Normal;
        public DefaultScreenContentType Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (!IsEnable)
                {
                    return;
                }
                if (!Enum.IsDefined(typeof(DefaultScreenContentType), (int)_type))
                {
                    throw new ArgumentNullException(nameof(Type), "不受支持默认显示屏幕类型");
                }
                _type = value;
            }
        }

        /// <summary>
        /// 默认会展示出来可以访问地址中的地址
        /// </summary>
        public List<string> DefaultShowHosts { get; set; }

        /// <summary>
        /// 前缀匹配地址
        /// </summary>
        public List<string> HostPrefixMatch { get; set; }
    }

    public class BemfaIot
    {
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 私钥
        /// </summary>
        public string PrivateKey { get; set; }
    }

    public class Database
    {

        public string Name { get; set; }

        public DataType Type { get; set; }

        public bool AutoSyncStructure { get; set; }

        private string _connectionString = null;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(ConnectionString), "Connection string can not null");
                }
                string connStr = value;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    connStr = connStr.Replace('/', Path.DirectorySeparatorChar);
                else
                    connStr = connStr.Replace('\\', Path.DirectorySeparatorChar);

                if (CommonHelper.TryParseLocalPathString(connStr, "%BASE%", AppContext.BaseDirectory, out string connTemp))
                {
                    connStr = connTemp;
                }
                _connectionString = connStr;
            }
        }
    }
}
