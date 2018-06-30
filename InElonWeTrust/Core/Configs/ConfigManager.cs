using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Configs
{
    public static class ConfigManager
    {
        public static ConfigContainer Data { get; private set; }

        private const string ConfigsPatch = "Configs/";

        static ConfigManager()
        {
            Load();
        }

        private static void Load()
        {
            using (var jsonReader = new StreamReader(GetConfigPatch()))
            {
                var jsonContent = jsonReader.ReadToEnd();
                Data = JsonConvert.DeserializeObject<ConfigContainer>(jsonContent);
            }
        }

        private static string GetConfigPatch()
        {
#if DEBUG
            var configName = "debug.json";
#else
            var configName = "release.json";
#endif

            return ConfigsPatch + configName;
        }
    }
}
