using System.Threading.Tasks;
using FFMpegCore;

namespace PiPlayer.AspNetCore.FFMpeg.Interface
{
    public interface IFFProbeService
    {
        Task<IMediaAnalysis> Analyse(string filePath);
    }
}
