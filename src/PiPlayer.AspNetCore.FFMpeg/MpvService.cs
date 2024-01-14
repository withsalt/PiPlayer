using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using FFMpegCore;
using Microsoft.Extensions.Logging;
using PiPlayer.AspNetCore.FFMpeg.Interface;
using PiPlayer.AspNetCore.FFMpeg.Models;

namespace PiPlayer.AspNetCore.FFMpeg
{
    public class MpvService : BaseFFPlayService, IMpvService
    {
        private readonly ILogger<MpvService> _logger;

        public MpvService(ILogger<MpvService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetBinaryPath()
        {
            string binName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                binName = "mpv.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                binName = "mpv";
            }
            else
            {
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");
            }

            string path = Path.Combine(GetBinaryFolder(), binName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("mpv not found, please download mpv in target os path.", path);
            }
            return path;
        }

        public async Task<LibVersion> GetVersion()
        {
            LibVersion libVersion = new LibVersion()
            {
                Status = false
            };
            try
            {
                var result = await Cli.Wrap(GetBinaryPath())
                    .WithArguments("--version")
                    .WithWorkingDirectory(GetBinaryFolder())
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync();
                libVersion.Status = true;
                if (result.ExitCode != 0)
                {
                    return libVersion;
                }
                string output = result.StandardOutput;
                if (string.IsNullOrEmpty(output))
                {
                    return libVersion;
                }
                if (output.StartsWith("mpv v", StringComparison.Ordinal))
                {
                    int startIndex = "mpv v".Length;
                    var lastIndex = output.IndexOf(' ', startIndex);
                    if (lastIndex > startIndex + 1)
                    {
                        libVersion.Version = output.Substring(startIndex - 1, lastIndex - startIndex + 1);
                    }
                }
                if (output.StartsWith("mpv ", StringComparison.Ordinal) && output.Contains("Copyright"))
                {
                    int startIndex = "mpv ".Length;
                    var lastIndex = output.IndexOf(' ', startIndex);
                    if (lastIndex > startIndex + 1)
                    {
                        libVersion.Version = output.Substring(startIndex - 1, lastIndex - startIndex + 1);
                    }
                }
                return libVersion;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Try get mpv version failed. {ex.Message}");
                return libVersion;
            }
        }
    }
}
