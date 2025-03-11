using System;
using System.IO;
using System.Reflection;
using FreeSql;
using Microsoft.Data.Sqlite;

namespace PiPlayer.DependencyInjection.Extensions
{
    internal static class FreeSqlCreateDatabaseExtension
    {
        /// <summary>
        /// 请在UseConnectionString配置后调用此方法
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static FreeSqlBuilder CreateDatabaseIfNotExists(this FreeSqlBuilder @this)
        {
            FieldInfo dataTypeFieldInfo = @this.GetType().GetField("_dataType", BindingFlags.NonPublic | BindingFlags.Instance);

            if (dataTypeFieldInfo is null)
            {
                throw new ArgumentException("_dataType is null");
            }

            string connectionString = GetConnectionString(@this);
            DataType dbType = (DataType)dataTypeFieldInfo.GetValue(@this);

            switch (dbType)
            {
                case DataType.MySql:
                    break;
                case DataType.SqlServer:
                    break;
                case DataType.PostgreSQL:
                    break;
                case DataType.Oracle:
                    break;
                case DataType.Sqlite:
                    return @this.CreateDatabaseIfNotExistsSqlite(connectionString);
                case DataType.Dameng:
                    break;
                case DataType.MsAccess:
                    break;
                case DataType.ShenTong:
                    break;
                case DataType.KingbaseES:
                    break;
                case DataType.Firebird:
                    break;
                default:
                    break;
            }

            throw new Exception($"不支持创建数据库");
        }

        public static FreeSqlBuilder CreateDatabaseIfNotExistsSqlite(this FreeSqlBuilder @this,
            string connectionString = "")
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(connectionString);
            FileInfo file = new FileInfo(builder.DataSource);
            if (file.Exists)
            {
                return @this;
            }
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            return @this;
        }

        #region NotSupport

        //        public static FreeSqlBuilder CreateDatabaseIfNotExistsMySql(this FreeSqlBuilder @this,
        //string connectionString = "")
        //        {
        //            if (connectionString == "")
        //            {
        //                connectionString = GetConnectionString(@this);
        //            }
        //            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);
        //            string createDatabaseSql = $"USE mysql;CREATE DATABASE IF NOT EXISTS `{builder.Database}` CHARACTER SET '{builder.CharacterSet}' COLLATE 'utf8mb4_general_ci'";
        //            string queryDbSql = "SHOW DATABASES";
        //            using MySqlConnection cnn = new MySqlConnection(
        //                $"Data Source={builder.Server};Port={builder.Port};User ID={builder.UserID};Password={builder.Password};Initial Catalog=;Charset=utf8;SslMode=none;Max pool size=1");
        //            cnn.Open();
        //            //判断数据库是否存在
        //            using (MySqlCommand cmd = cnn.CreateCommand())
        //            {
        //                cmd.CommandText = queryDbSql;
        //                using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
        //                {
        //                    using DataSet dataset = new DataSet();
        //                    adapter.Fill(dataset);

        //                    using DataTable dt = dataset.Tables[0];
        //                    DataRowCollection rows = dt.Rows;
        //                    foreach (DataRow dr in rows)
        //                    {
        //                        string dbName = dr[0].ToString();
        //                        if (dbName.Equals(builder.Database, StringComparison.OrdinalIgnoreCase))
        //                            return @this;
        //                    }
        //                }
        //            }
        //            using (MySqlCommand cmd = cnn.CreateCommand())
        //            {
        //                cmd.CommandText = createDatabaseSql;
        //                cmd.ExecuteNonQuery();
        //            }

        //            return @this;
        //        }

        //        public static FreeSqlBuilder CreateDatabaseIfNotExistsSqlServer(this FreeSqlBuilder @this, string connectionString = "")
        //        {
        //            if (connectionString == "")
        //            {
        //                connectionString = GetConnectionString(@this);
        //            }
        //            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
        //            string createDatabaseSql;
        //            if (!string.IsNullOrEmpty(builder.AttachDBFilename))
        //            {
        //                string fileName = ExpandFileName(builder.AttachDBFilename);
        //                string name = Path.GetFileNameWithoutExtension(fileName);
        //                string logFileName = Path.ChangeExtension(fileName, ".ldf");
        //                createDatabaseSql = @$"CREATE DATABASE {builder.InitialCatalog}   on  primary   
        //                (
        //                    name = '{name}',
        //                    filename = '{fileName}'
        //                )
        //                log on
        //                (
        //                    name= '{name}_log',
        //                    filename = '{logFileName}'
        //                )";
        //            }
        //            else
        //            {
        //                createDatabaseSql = @$"CREATE DATABASE {builder.InitialCatalog}";
        //            }

        //            using SqlConnection cnn =
        //                new SqlConnection(
        //                    $"Data Source={builder.DataSource};Integrated Security = True;User ID={builder.UserID};Password={builder.Password};Initial Catalog=master;Min pool size=1;Encrypt={builder.Encrypt};TrustServerCertificate={builder.TrustServerCertificate};Trusted_Connection=False;");
        //            cnn.Open();
        //            using SqlCommand cmd = cnn.CreateCommand();
        //            cmd.CommandText = $"select * from sysdatabases where name = '{builder.InitialCatalog}'";

        //            SqlDataAdapter apter = new SqlDataAdapter(cmd);
        //            DataSet ds = new DataSet();
        //            apter.Fill(ds);

        //            if (ds.Tables[0].Rows.Count == 0)
        //            {
        //                cmd.CommandText = createDatabaseSql;
        //                cmd.ExecuteNonQuery();
        //            }

        //            return @this;
        //        }

        //        public static FreeSqlBuilder CreateDatabaseIfNotExistsPgSql(this FreeSqlBuilder @this,
        //      string connectionString = "")
        //        {
        //            if (connectionString == "")
        //            {
        //                connectionString = GetConnectionString(@this);
        //            }
        //            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString);
        //            string createDatabaseSql = $"DROP DATABASE IF EXISTS \"{builder.Database}\"; CREATE DATABASE \"{builder.Database}\" WITH OWNER = \"{builder.Username}\"";
        //            string queryDbSql = "SELECT datname FROM pg_database";
        //            using NpgsqlConnection cnn = new NpgsqlConnection(
        //                $"Host={builder.Host};Port={builder.Port};Database=postgres;Username={builder.Username};Password={builder.Password};Pooling=true");
        //            cnn.Open();
        //            //判断数据库是否存在
        //            using (NpgsqlCommand cmd = cnn.CreateCommand())
        //            {
        //                cmd.CommandText = queryDbSql;
        //                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
        //                {
        //                    using DataSet dataset = new DataSet();
        //                    adapter.Fill(dataset);

        //                    using DataTable dt = dataset.Tables[0];
        //                    DataRowCollection rows = dt.Rows;
        //                    foreach (DataRow dr in rows)
        //                    {
        //                        string dbName = dr[0].ToString();
        //                        if (dbName.Equals(builder.Database, StringComparison.OrdinalIgnoreCase))
        //                            return @this;
        //                    }
        //                }
        //            }
        //            //创建数据库
        //            using (NpgsqlCommand cmd = cnn.CreateCommand())
        //            {
        //                cmd.CommandText = createDatabaseSql;
        //                cmd.ExecuteReader();
        //            }
        //            return @this;
        //        }

        #endregion



        #region helper


        private static string ExpandFileName(string fileName)
        {
            if (fileName.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }
                string name = fileName.Replace("\\", "").Replace("/", "").Substring("|DataDirectory|".Length);
                fileName = Path.Combine(dataDirectory, name);
            }
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            return Path.GetFullPath(fileName);
        }


        private static string GetConnectionString(FreeSqlBuilder @this)
        {
            Type type = @this.GetType();
            FieldInfo fieldInfo =
                type.GetField("_masterConnectionString", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo is null)
            {
                throw new ArgumentException("_masterConnectionString is null");
            }
            return fieldInfo.GetValue(@this).ToString();
        }

        #endregion
    }
}