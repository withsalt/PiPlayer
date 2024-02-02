using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using PiPlayer.AspNetCore.FFMpeg.Interface;
using PiPlayer.Configs;
using PiPlayer.Models.Common;
using PiPlayer.Models.Common.JsonObject;
using PiPlayer.Models.Entities;
using PiPlayer.Models.Enums;
using PiPlayer.Models.ViewModels.Request.Upload;
using PiPlayer.Models.ViewModels.Response.FileUpload;
using PiPlayer.Repository.Interface;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PiPlayer.Controllers
{
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> _logger;
        private readonly ConfigManager _config;
        private readonly IMediaRepository _repository;
        private readonly IFFProbeService _ffprobeService;
        private readonly IFFMpegService _ffmpegService;

        public UploadController(ILogger<UploadController> logger
            , IWebHostEnvironment hostingEnvironment
            , ConfigManager config
            , IMediaRepository repository
            , IFFProbeService ffprobeService
            , IFFMpegService ffmpegService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<UploadController>));
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));
            _repository = repository ?? throw new ArgumentNullException(nameof(IMediaRepository));
            _ffprobeService = ffprobeService ?? throw new ArgumentNullException(nameof(IFFProbeService));
            _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(IFFMpegService));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(UploadParams uploadParams)
        {
            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                return Json(new UploadResult()
                {
                    error = "上传文件为空。"
                });
            }
            foreach (var item in files)
            {
                uploadParams.uploadFiles.Add(item);
            }
            foreach (var item in uploadParams.uploadFiles)
            {
                string fileName = item.FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return Json(new UploadResult()
                    {
                        error = "上传文件文件名不能为空。"
                    });
                }
                string ext = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(ext))
                {
                    return Json(new UploadResult()
                    {
                        error = "后缀名不能为空。"
                    });
                }
                FileType fileType = FileType.Unknow;
                if (_config.AppSettings.AllowExtensions != null && _config.AppSettings.AllowExtensions.Count > 0)
                {
                    foreach (var fileTypeItem in _config.AppSettings.AllowExtensions)
                    {
                        if (fileTypeItem.Values != null && fileTypeItem.Values.Contains(ext.ToLower()))
                        {
                            fileType = fileTypeItem.Type;
                            break;
                        }
                    }
                }
                if (fileType == FileType.Unknow)
                {
                    return Json(new UploadResult()
                    {
                        error = "不支持的文件后后缀名。"
                    });
                }

                ext = ext.ToLower();
                string fileNewName = Guid.NewGuid().ToString("n") + ext;
                (string, string) fileSaveResult = await SaveFile(item, fileNewName);
                if (string.IsNullOrEmpty(fileSaveResult.Item1))
                {
                    return Json(new UploadResult()
                    {
                        error = "保存文件失败。"
                    });
                }
                try
                {
                    string logoPath = null;
                    int duration = 0, width = 0, height = 0;
                    switch (fileType)
                    {
                        case FileType.Video:
                            var videoInfo = await _ffprobeService.Analyse(fileSaveResult.Item2);
                            if (videoInfo != null && videoInfo.VideoStreams?.Any() == true)
                            {
                                width = videoInfo.VideoStreams[0].Width;
                                height = videoInfo.VideoStreams[0].Height;
                                duration = (int)videoInfo.VideoStreams[0].Duration.TotalSeconds;
                                if (duration <= 2)
                                {
                                    throw new Exception("视频时长太短了，时长不能小于2s");
                                }
                                //生成略缩图
                                logoPath = await VideoThumbnailGenerator(fileSaveResult.Item2, 640, 480, duration / 2);
                                if (string.IsNullOrEmpty(logoPath))
                                {
                                    logoPath = $"assets/img/logo/video.png";
                                }
                            }
                            else
                            {
                                logoPath = $"assets/img/logo/video.png";
                            }

                            break;
                        case FileType.Image:
                            (string, int, int) imageTn = await ImageThumbnailGenerator(fileSaveResult.Item2);
                            if (string.IsNullOrEmpty(imageTn.Item1))
                            {
                                logoPath = $"assets/img/logo/image.png";
                            }
                            else
                            {
                                logoPath = imageTn.Item1;
                                width = imageTn.Item2;
                                height = imageTn.Item3;
                            }
                            break;
                        case FileType.Music:
                            var audioInfo = await _ffprobeService.Analyse(fileSaveResult.Item2);
                            if (audioInfo != null && audioInfo.AudioStreams?.Any() == true)
                            {
                                duration = (int)audioInfo.AudioStreams[0].Duration.TotalSeconds;
                            }
                            else
                            {
                                duration = 0;
                            }
                            logoPath = $"assets/img/logo/music.png";
                            break;
                        default:
                            break;
                    }
                    FileUploadParam fileUploadModel = new FileUploadParam()
                    {
                        FileName = fileNewName,
                        FileOldName = fileName,
                        Path = fileSaveResult.Item1,
                        Logo = logoPath,
                        Extension = ext,
                        Duration = duration,
                        FileType = fileType,
                        Width = width,
                        Height = height,
                        Size = item.Length / 1024
                    };
                    if (!await _repository.SaveFileInfo(fileUploadModel))
                    {
                        return Json(new UploadResult()
                        {
                            error = "写入文件信息到数据库失败。"
                        });
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (System.IO.File.Exists(fileSaveResult.Item2))
                        {
                            System.IO.File.Delete(fileSaveResult.Item2);
                        }
                    }
                    catch { }
                    _logger.LogError(ex, $"文件上传失败，{ex.Message}");
                    return Json(new UploadResult()
                    {
                        error = $"上传失败。{ex.Message}"
                    });
                }
            }

            //上传成功
            return Json(new UploadResult());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new Exception("文件ID不能为空。");
                }
                Media item = await _repository.FindAsync(id);
                if (item == null)
                {
                    throw new Exception("查找文件信息失败。");
                }
                int result = await _repository.DeleteAsync(id);
                if (result <= 0)
                {
                    throw new Exception("删除文件失败。");
                }
                string filePath = Path.Combine(_config.AppSettings.DataDirectory, item.Path);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                if (!string.IsNullOrEmpty(item.LogoUrl) && item.LogoUrl.StartsWith(GlobalConfigConstant.DefaultMediaDirectory + "/"))
                {
                    string logoPath = Path.Combine(_config.AppSettings.DataDirectory, item.LogoUrl);
                    if (System.IO.File.Exists(logoPath))
                    {
                        System.IO.File.Delete(logoPath);
                    }
                }
                return Json(new ResultModel<IChild>(0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delete file by id({id}) failed. {ex.Message}");
                return Json(new ResultModel<IChild>(1001, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> FileInfo(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new ResultModel<IChild>(10031, "文件ID不能为空。"));
                }
                Media item = await _repository.FindAsync(id);
                if (item == null)
                {
                    return Json(new ResultModel<IChild>(10033, "查找文件信息失败。"));
                }
                string baseHost = $"{Request.Scheme}://{Request.Host}/";
                item.LogoUrl = baseHost + item.LogoUrl;

                string contentType = "application/octet-stream";
                string fileName = Path.GetFileName(item.FileOldName);
                if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType))
                {
                    contentType = "application/octet-stream";
                }

                return Json(new ResultModel<FileInfoResult>(0)
                {
                    Data = new FileInfoResult()
                    {
                        Id = item.Id,
                        FileName = item.FileName,
                        FileOldName = item.FileOldName,
                        Path = item.Path,
                        LogoPath = item.LogoUrl,
                        Duration = item.Duration,
                        Extension = item.Extension,
                        FileType = item.FileType,
                        Size = item.Size,
                        Remark = item.Remark,
                        CreatedTime = item.CreatedTime,
                        ContentType = contentType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delete file by id({id}) failed. {ex.Message}");
                return Json(new ResultModel<IChild>(10031, ex.Message));
            }
        }

        private async Task<(string, string)> SaveFile(IFormFile formFile, string fileNewName)
        {
            try
            {
                if (formFile == null)
                {
                    return (null, null);
                }
                if (string.IsNullOrEmpty(fileNewName))
                {
                    return (null, null);
                }
                string dataPath = Path.Combine(GlobalConfigConstant.DefaultMediaDirectory, DateTime.Now.ToString("yyyyMMdd"));
                string savePath = Path.Combine(_config.AppSettings.DataDirectory, dataPath);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fileName = Path.Combine(savePath, fileNewName);
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    var inputStream = formFile.OpenReadStream();
                    await inputStream.CopyToAsync(fileStream, 80 * 1024, default);
                }
                if (System.IO.File.Exists(fileName))
                {
                    return (Path.Combine(dataPath, fileNewName).Replace(Path.DirectorySeparatorChar, '/'), fileName);
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Save upload file failed. {ex.Message}");
                return (null, null);
            }
        }

        /// <summary>
        /// 生成图片略缩图
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<(string, int, int)> ImageThumbnailGenerator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return (null, 0, 0);
            }
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    throw new Exception("Process img is not exist.");
                }
                string fileName = Path.GetFileNameWithoutExtension(path);
                string imgDestPath = Path.Combine(GlobalConfigConstant.DefaultMediaDirectory, DateTime.Now.ToString("yyyyMMdd"), $"{fileName}_logo.png");
                string imgDestFullPath = Path.Combine(_config.AppSettings.DataDirectory, imgDestPath);
                if (System.IO.File.Exists(imgDestFullPath))
                {
                    System.IO.File.Delete(imgDestFullPath);
                }
                int width = 640;
                int height = 480;
                using (var image = Image.Load<Rgba32>(path))
                {
                    image.Mutate(x => x.Resize(width, height));
                    await image.SaveAsPngAsync(imgDestFullPath);
                }

                if (System.IO.File.Exists(imgDestFullPath))
                {
                    return (imgDestPath.Replace("\\", "/").TrimStart('/'), width, height);
                }
                else
                {
                    return (null, width, height);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Build thumbNail image failed, {ex.Message}");
                return (null, 0, 0);
            }
        }

        /// <summary>
        /// 从视频画面中截取一帧画面为图片
        /// </summary>
        /// <param name="videoName">视频文件路径pic/123.MP4</param>
        /// <param name="widthAndHeight">图片的尺寸如:240*180</param>
        /// <param name="cutTimeFrame">开始截取的时间如:"1s"</param>
        /// <returns>返回图片保存路径</returns>
        private async Task<string> VideoThumbnailGenerator(string videoPath, int width, int height, int cutTime = 5)
        {
            try
            {
                if (string.IsNullOrEmpty(videoPath))
                {
                    return null;
                }
                string fileName = Path.GetFileNameWithoutExtension(videoPath);
                string imgDestPath = Path.Combine(GlobalConfigConstant.DefaultMediaDirectory, DateTime.Now.ToString("yyyyMMdd"), $"{fileName}_logo.png");
                string imgDestFullPath = Path.Combine(_config.AppSettings.DataDirectory, imgDestPath);
                if (System.IO.File.Exists(imgDestFullPath))
                {
                    System.IO.File.Delete(imgDestFullPath);
                }
                await _ffmpegService.Snapshot(videoPath, imgDestFullPath, width, height, cutTime);
                if (System.IO.File.Exists(imgDestFullPath))
                {
                    return imgDestPath.Replace("\\", "/").TrimStart('/');
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Create video logo failed. {ex.Message}");
                return null;
            }
        }


    }
}
