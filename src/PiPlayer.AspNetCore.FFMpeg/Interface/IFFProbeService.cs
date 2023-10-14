using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore;

namespace PiPlayer.AspNetCore.FFMpeg.Interface
{
    public interface IFFProbeService
    {
        Task<IMediaAnalysis> Analyse(string filePath);
    }
}
