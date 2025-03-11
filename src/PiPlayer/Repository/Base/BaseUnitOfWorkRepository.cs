﻿using System;
using System.Linq.Expressions;
using FreeSql;

namespace PiPlayer.Repository.Base
{
    public abstract class BaseUnitOfWorkRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class
    {
        public IdleBus<IFreeSql> DbContainer { get; set; }

        public BaseUnitOfWorkRepository(BaseUnitOfWorkManager uow, Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) : base(uow.Orm)
        {
            uow.Binding(this);
            DbContainer = uow.DbContainer;
        }
    }
}
