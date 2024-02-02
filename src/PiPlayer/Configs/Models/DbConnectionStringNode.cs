using FreeSql;

namespace PiPlayer.Configs.Models
{
    public class DbConnectionStringNode
    {
        /// <summary>
        /// 标识名称
        /// </summary>
        public string Name { get; set; }

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
        public string ConnectionString { get; set; }

        /// <summary>
        /// 从库
        /// </summary>
        public string[] Slaves { get; set; }
    }
}
