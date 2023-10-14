using PiPlayer.Models.Enums;

namespace PiPlayer.Models.ViewModels.Request.Upload
{
    public class FileUploadParam
    {
        public string FileName { get; set; }

        public string FileOldName { get; set; }

        public string Path { get; set; }

        public string Logo { get; set; }

        public string Extension { get; set; }

        public int Duration { get; set; }

        public FileType FileType { get; set; }

        public int Height { get; set; } = 0;

        public int Width { get; set; } = 0;

        public float Size { get; set; }

        public string Remark { get; set; }
    }
}
