using System.Threading.Tasks;
using FreeSql;
using PiPlayer.Models.Entities;
using PiPlayer.Models.ViewModels.Request.Upload;
using PiPlayer.Repository.Base;

namespace PiPlayer.Repository.Interface
{
    public interface IMediaRepository : IBaseRepository<Media, long>, IRepositoryDependency
    {
        Task<bool> SaveFileInfo(FileUploadParam uploadParam);
    }
}
