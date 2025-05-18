using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using FreeSql;
using FreeSql.Aop;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using PiPlayer.Configs;
using PiPlayer.Configs.Models;
using PiPlayer.DependencyInjection.Extensions;
using PiPlayer.Models.Attributes;
using PiPlayer.Models.Entities;
using PiPlayer.Repository;
using PiPlayer.Repository.Base;
using PiPlayer.Repository.Interface;

namespace PiPlayer.DependencyInjection
{
    public static class RegisterDatabase
    {
        static readonly IdleBus<IFreeSql> ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(60));

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                ILogger<IFreeSql> logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<IFreeSql>();
                ConfigManager config = provider.GetRequiredService<ConfigManager>();
                IIdGeneratorService idGenerator = provider.GetRequiredService<IIdGeneratorService>();
                IWebHostEnvironment env = provider.GetRequiredService<IWebHostEnvironment>();
                //获取所有的连接字符串
                DbConnectionStringNode dbSetting = GetConnectionStrings(config);
                //创建IFreeSql对象
                var registerResult = ib.TryRegister(dbSetting.Name, () =>
                {
                    //是否开启自动迁移
                    bool syncStructure = IsAutoSyncStructure(dbSetting, env);
                    //create builder
                    FreeSqlBuilder fsqlBuilder = new FreeSqlBuilder()
                        .UseConnectionString(dbSetting.DbType, dbSetting.ConnectionString)
                        .UseAutoSyncStructure(syncStructure);
                    //如果数据库不存在，那么自动创建数据库
                    if (syncStructure && (dbSetting.DbType == FreeSql.DataType.MySql
                        || dbSetting.DbType == FreeSql.DataType.SqlServer
                        || dbSetting.DbType == FreeSql.DataType.PostgreSQL
                        || dbSetting.DbType == FreeSql.DataType.Sqlite
                        || dbSetting.DbType == FreeSql.DataType.OdbcSqlServer))
                    {
                        fsqlBuilder.CreateDatabaseIfNotExists();
                    }
                    //判断是否开启读写分离
                    if (dbSetting.Slaves != null && dbSetting.Slaves.Length > 0)
                    {
                        fsqlBuilder.UseSlave(dbSetting.Slaves);
                    }
                    IFreeSql fsql = fsqlBuilder.Build();
                    //sql执行日志
                    fsql.Aop.CurdAfter += (s, e) =>
                    {
                        logger.LogDebug($"{dbSetting.Name}(thread-{Thread.CurrentThread.ManagedThreadId}):\n  Namespace: {e.EntityType.FullName} \nElapsedTime: {e.ElapsedMilliseconds}ms \n        SQL: {e.Sql}");
                    };
                    //审计
                    fsql.Aop.AuditValue += (s, e) =>
                    {
                        //插入操作，如果是long类型的主键为0，则生成雪花Id
                        if ((e.AuditValueType == AuditValueType.Insert || e.AuditValueType == AuditValueType.InsertOrUpdate)
                         && e.Column.CsType == typeof(long)
                         && e.Value?.ToString().Equals("0") == true
                         && (e.Property.GetCustomAttribute<KeyAttribute>(false) != null || (e.Property.GetCustomAttribute<ColumnAttribute>(false)?.IsPrimary == true)))
                        {
                            //生成雪花Id
                            e.Value = idGenerator.CreateId();
                        }
                    };
                    return fsql;
                });
                if (!registerResult)
                {
                    throw new Exception($"Register db '{dbSetting.Name}' failed.");
                }

                //注入
                services.AddScoped<BaseUnitOfWorkManager>();
                //注入IdleBus<IFreeSql>
                services.TryAddSingleton(ib);
                return services;
            }
        }

        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();
            return services;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app)
        {
            IdleBus<IFreeSql> ib = app.ApplicationServices.GetRequiredService<IdleBus<IFreeSql>>();
            if (ib == null)
            {
                throw new Exception("Get idlebus service failed.");
            }
            ConfigManager config = app.ApplicationServices.GetRequiredService<ConfigManager>();
            IWebHostEnvironment env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            ILoggerFactory loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(nameof(RegisterDatabase));
            //获取所有的连接字符串
            DbConnectionStringNode connectionString = GetConnectionStrings(config);
            if (!connectionString.AutoSyncStructure)
            {
                return app;
            }
            //配置文件中开启了初始化数据库，并开启了开发者模式
            if (!IsAutoSyncStructure(connectionString, env))
            {
                return app;
            }

            IFreeSql db = ib.Get(connectionString.Name);
            if (db == null)
            {
                throw new Exception($"Can not get db '{connectionString.Name}' from IdleBus.");
            }
            logger.LogInformation($"For db '{connectionString.Name}', automatic sync database structure is turned on, start seeding database...");
            //同步表结构
            SyncStructure(db);
            //写入种子数据
            //db.Transaction(() =>
            //{

            //});

            return app;
        }

        /// <summary>
        /// 同步表结构
        /// </summary>
        /// <param name="fsql"></param>
        private static void SyncStructure(IFreeSql fsql)
        {
            if (!fsql.CodeFirst.IsAutoSyncStructure)
            {
                return;
            }
            List<Type> tableAssembies = new List<Type>();
            var entities = GetExportedTypesByInterface(typeof(IEntity));
            foreach (Type type in entities)
            {
                if (type.GetCustomAttribute<TableAttribute>() != null
                    && type.GetCustomAttribute<DisableSyncStructureAttribute>() == null
                    && type.BaseType != null
                    && (type.BaseType == typeof(IBaseEntity)
                    || type.BaseType == typeof(IBaseEntity<long>)
                    || type.BaseType == typeof(IBaseEntity<int>)))
                {
                    tableAssembies.Add(type);
                }
            }
            if (tableAssembies.Count == 0)
            {
                return;
            }
            fsql.CodeFirst.SyncStructure(tableAssembies.ToArray());
        }


        private static List<Type> GetExportedTypesByInterface(Type interfaceType, bool allowInterface = false)
        {
            List<Type> result = new List<Type>();
            List<Type> allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes()).ToList();
            foreach (var p in allTypes)
            {
                Type[] interfaceTypes = p.GetInterfaces();
                if (interfaceTypes == null || interfaceTypes.Length == 0)
                {
                    continue;
                }
                if (allowInterface)
                {
                    if (interfaceTypes.Contains(interfaceType))
                    {
                        result.Add(p);
                    }
                }
                else
                {
                    if (interfaceTypes.Contains(interfaceType) && !p.IsInterface && !p.IsAbstract && p.IsClass)
                    {
                        result.Add(p);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 是否开启表结构同步
        /// </summary>
        /// <param name="dbConfig"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        private static bool IsAutoSyncStructure(DbConnectionStringNode dbConfig, IWebHostEnvironment env)
        {
            return dbConfig.AutoSyncStructure;
        }

        /// <summary>
        /// 从IConfiguration中获取连接字符串
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static DbConnectionStringNode GetConnectionStrings(ConfigManager config)
        {
            var dbSetting = config.AppSettings.Database;
            if (string.IsNullOrWhiteSpace(dbSetting.ConnectionString))
            {
                throw new Exception("Can not get connect string from app setting.");
            }
            return new DbConnectionStringNode()
            {
                Name = dbSetting.Name,
                DbType = dbSetting.Type,
                ConnectionString = dbSetting.ConnectionString,
                AutoSyncStructure = dbSetting.AutoSyncStructure,
            };
        }
    }
}
