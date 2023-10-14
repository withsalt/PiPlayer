using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore;

namespace PiPlayer.AspNetCore.FFMpeg
{
    public abstract class BaseFFPlayService
    {
        public string GetBinaryFolder()
        {
            return GlobalFFOptions.Current.BinaryFolder;
        }

        public string GeTemporaryFilesFolder()
        {
            return GlobalFFOptions.Current.TemporaryFilesFolder;
        }
    }
}
