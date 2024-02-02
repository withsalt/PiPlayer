using System.Threading.Tasks;
using PiPlayer.AspNetCore.FFMpeg.Models;

namespace PiPlayer.AspNetCore.FFMpeg.Interface
{
    public interface IFFMpegService
    {
        Task<bool> Snapshot(string filePath, string outPath, int width, int height, int cutTime);

        string GetBinaryPath();

        Task<LibVersion> GetVersion();
    }
}
