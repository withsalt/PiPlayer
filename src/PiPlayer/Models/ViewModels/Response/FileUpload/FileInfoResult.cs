using PiPlayer.Models.Common.JsonObject;
using PiPlayer.Models.Enums;
using System;

namespace PiPlayer.Models.ViewModels.Response.FileUpload
{
    public class FileInfoResult : IChild
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public string FileSource { get; set; }

        public string FileOldName { get; set; }

        public string Path { get; set; }

        public string LogoPath { get; set; }

        public int? Duration { get; set; } = 0;

        public string Extension { get; set; }

        public FileType FileType { get; set; }

        public float Size { get; set; }

        public string Remark { get; set; }

        public DateTime? CreatedTime { get; set; }

        public string ContentType { get; set; }
    }
}
