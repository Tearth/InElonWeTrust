using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InElonWeTrust.Core.Services.Changelog
{
    public class ChangelogService
    {
        private readonly HttpClient _httpClient;
        private const string ReadmeUrl = "https://raw.githubusercontent.com/Tearth/InElonWeTrust/master/CHANGELOG.md";

        public ChangelogService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetChangelog()
        {
            var changelog = await _httpClient.GetStringAsync(ReadmeUrl);
            var latestContent = changelog.Substring(0, 1000);
            var latestRecord = latestContent.LastIndexOf("**v", StringComparison.InvariantCulture);

            return new string(latestContent.Take(latestRecord).ToArray());
        }
    }
}
