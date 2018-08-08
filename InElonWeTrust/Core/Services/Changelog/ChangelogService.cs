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
            return await _httpClient.GetStringAsync(ReadmeUrl);
        }
    }
}
