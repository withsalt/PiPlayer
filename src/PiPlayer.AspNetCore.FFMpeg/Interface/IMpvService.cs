using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PiPlayer.AspNetCore.FFMpeg.Models;

namespace PiPlayer.AspNetCore.FFMpeg.Interface
{
    public interface IMpvService
    {
        string GetBinaryPath();

        Task<LibVersion> GetVersion();
    }
}
