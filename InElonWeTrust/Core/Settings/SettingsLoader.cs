using System.IO;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Settings
{
    public static class SettingsLoader
    {
        public static SettingsContainer Data { get; private set; }
        private const string ConfigsPatch = "Settings/";

        static SettingsLoader()
        {
            Load();
        }

        public static void Load()
        {
            using (var jsonReader = new StreamReader(GetConfigPatch()))
            {
                var jsonContent = jsonReader.ReadToEnd();
                Data = JsonConvert.DeserializeObject<SettingsContainer>(jsonContent);
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
