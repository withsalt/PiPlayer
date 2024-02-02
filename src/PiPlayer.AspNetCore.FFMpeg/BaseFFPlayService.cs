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
