using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using PiPlayer.AspNetCore.FFMpeg.Interface;

namespace PiPlayer.AspNetCore.FFMpeg
{
    public class FFPlayService : BaseFFPlayService, IFFPlayService
    {
        private readonly ILogger<FFPlayService> _logger;

        public FFPlayService(ILogger<FFPlayService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetBinaryPath()
        {
            string binName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                binName = "ffplay.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                binName = "ffplay";
            }
            else
            {
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");
            }

            string path = Path.Combine(GetBinaryFolder(), binName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("ffplay not found, please download ffplay in target os path.", path);
            }
            return path;
        }
    }
}
