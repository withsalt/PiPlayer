using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiPlayer.Models.ViewModels.Response.FileUpload
{
    public class UploadParams
    {
        public string fileId { get; set; }

        public string fileName { get; set; }

        public double fileSize { get; set; }

        public List<IFormFile> uploadFiles { get; set; } = new List<IFormFile>();
    }
}
