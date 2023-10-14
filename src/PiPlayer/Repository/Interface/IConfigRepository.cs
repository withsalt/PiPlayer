using FreeSql;
using PiPlayer.Models.Entities;
using PiPlayer.Repository.Base;

namespace PiPlayer.Repository.Interface
{
    public interface IConfigRepository : IBaseRepository<Config, long>, IRepositoryDependency
    {

    }
}
