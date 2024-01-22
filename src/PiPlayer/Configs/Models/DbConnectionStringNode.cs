using System;
using System.IO;
using System.Runtime.InteropServices;
using FreeSql;
using PiPlayer.Utils;

namespace PiPlayer.Configs.Models
{
    public class DbConnectionStringNode
    {
        private string _connectionString = null;
        private string _name = null;

        /// <summary>
        /// 标识名称
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(this.Name), "The connect string name can not null.");
                }
                _name = value;
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DataType DbType { get; set; }

        /// <summary>
        /// 是否自动同步结构
        /// </summary>
        public bool AutoSyncStructure { get; set; } = false;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(ConnectionString), "Connection string can not null");
                }

                if (DbType == DataType.Sqlite)
                {
                    string connStr = value;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        connStr = connStr.Replace('/', Path.DirectorySeparatorChar);
                    else
                        connStr = connStr.Replace('\\', Path.DirectorySeparatorChar);

                    if (CommonHelper.TryParseLocalPathString(connStr, "%BASE%", AppContext.BaseDirectory, out string connTemp))
                    {
                        connStr = connTemp;
                    }
                    _connectionString = connStr;
                }
                else
                {
                    _connectionString = value;
                }
            }
        }

        /// <summary>
        /// 从库
        /// </summary>
        public string[] Slaves { get; set; }
    }
}
