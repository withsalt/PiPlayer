using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PiPlayer.Configs.Models;

namespace PiPlayer.Configs
{
    public class ConfigManager
    {
        private static ConfigManager _manager = null;
        private static readonly object _locker = new object();

        [JsonIgnore]
        public IConfiguration Configuration { get; private set; }

        public ConfigManager(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(IConfiguration));
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public AppSettingsNode AppSettings
        {
            get
            {
                return GetSection<AppSettingsNode>();
            }
        }

        public static void LoadAppsettings(HostBuilderContext hostingContext, IConfigurationBuilder configuration)
        {
            string normalConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(normalConfigPath))
            {
                configuration.AddJsonFile(normalConfigPath, optional: false, reloadOnChange: true);
            }

            string eventName = hostingContext.HostingEnvironment.EnvironmentName;
            if (!string.IsNullOrEmpty(eventName))
            {
                string eventAppConfigPath = Path.Combine(AppContext.BaseDirectory, $"appsettings.{eventName}.json");
                if (File.Exists(eventAppConfigPath))
                {
                    configuration.AddJsonFile(eventAppConfigPath, optional: false, reloadOnChange: true);
                }
            }
        }

        public void Load()
        {
            if (_manager == null)
            {
                lock (_locker)
                {
                    if (_manager == null)
                    {
                        _manager = this;
                    }
                }
            }
        }

        private T GetSection<T>(string sectionName = null)
        {
            string nameofT = typeof(T).Name;
            if (string.IsNullOrEmpty(nameofT) && string.IsNullOrEmpty(sectionName))
            {
                return default;
            }
            if (string.IsNullOrEmpty(sectionName))
            {
                int index = nameofT.LastIndexOf("Node");
                if (index > 0 && index == nameofT.Length - 4)
                {
                    nameofT = nameofT.Substring(0, index);
                }
            }
            else
            {
                nameofT = sectionName;
            }
            IConfigurationSection section = Configuration.GetSection(nameofT);
            if (section == null || !section.Exists())
            {
                return default;
            }
            return section.Get<T>();
        }

        /// <summary>
        /// 根据Key获取value
        /// Exp: AppSettings:OSS:AccessKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key);
            return Configuration.GetValue<string>(key);
        }
    }
}
