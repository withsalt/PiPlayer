using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PiPlayer.Models.Entities;
using PiPlayer.Models.ViewModels.Request.Upload;
using PiPlayer.Repository.Base;
using PiPlayer.Repository.Interface;

namespace PiPlayer.Repository
{
    public class MediaRepository : BaseUnitOfWorkRepository<Media, long>, IMediaRepository
    {
        private readonly ILogger<MediaRepository> _logger;
        private readonly IFreeSql _db;

        public MediaRepository(BaseUnitOfWorkManager uow
            , ILogger<MediaRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
        }

        public async Task<bool> SaveFileInfo(FileUploadParam uploadParam)
        {
            try
            {
                Media entity = new Media()
                {
                    FileName = uploadParam.FileName,
                    FileOldName = uploadParam.FileOldName,
                    Path = uploadParam.Path,
                    LogoUrl = uploadParam.Logo,
                    Duration = uploadParam.Duration,
                    Extension = uploadParam.Extension,
                    FileType = uploadParam.FileType,
                    Width = uploadParam.Width,
                    Height = uploadParam.Height,
                    Size = uploadParam.Size,
                    Remark = uploadParam.Remark,
                    CreatedTime = DateTime.Now,
                };
                var result = await this.InsertAsync(entity);
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Save file to db failed. {ex.Message}");
                return false;
            }
        }
    }
}
