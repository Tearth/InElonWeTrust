using System.IO;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Settings
{
    public static class SettingsLoader
    {
        public static SettingsContainer Data { get; private set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string ConfigsPatch = "Settings/";

        static SettingsLoader()
        {
            Load();
        }

        public static void Load()
        {
            var configPatch = GetConfigPatch();

            Logger.Info($"Loading {configPatch}...");
            using (var jsonReader = new StreamReader(configPatch))
            {
                var jsonContent = jsonReader.ReadToEnd();
                Data = JsonConvert.DeserializeObject<SettingsContainer>(jsonContent);
            }
        }

        private static string GetConfigPatch()
        {
#if DEBUG
            const string configName = "debug.json";
#else
            const var configName = "release.json";
#endif

            return ConfigsPatch + configName;
        }
    }
}
