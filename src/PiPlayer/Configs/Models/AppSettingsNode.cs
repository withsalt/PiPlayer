﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using PiPlayer.Utils;

namespace PiPlayer.Configs.Models
{
    public class AppSettingsNode
    {
        public List<AllowExtensionNode> AllowExtensions { get; set; }

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

        public bool IsEnableAP { get; set; }

        public string APName { get; set; }

        public string APPasswd { get; set; }

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

                if (CommonHelper.TryParseConnectionString(dic, "%BASE%", AppContext.BaseDirectory, out string connTemp))
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
    }

    public class ScreenMode
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public bool FullScreen { get; set; }
    }
}
