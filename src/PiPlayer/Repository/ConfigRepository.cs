using Microsoft.Extensions.Logging;
using PiPlayer.Models.Entities;
using PiPlayer.Repository.Base;
using PiPlayer.Repository.Interface;
using System;

namespace PiPlayer.Repository
{
    public class ConfigRepository : BaseUnitOfWorkRepository<Config, long>, IConfigRepository
    {
        private readonly ILogger<ConfigRepository> _logger;
        private readonly IFreeSql _db;

        public ConfigRepository(BaseUnitOfWorkManager uow
            , ILogger<ConfigRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
        }
    }
}
