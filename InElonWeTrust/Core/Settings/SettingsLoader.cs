using System.IO;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Configs
{
    public class SettingsLoader
    {
        private const string ConfigsPatch = "Configs/";

        public SettingsContainer Load()
        {
            using (var jsonReader = new StreamReader(GetConfigPatch()))
            {
                var jsonContent = jsonReader.ReadToEnd();
                return JsonConvert.DeserializeObject<SettingsContainer>(jsonContent);
            }
        }

        private string GetConfigPatch()
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
