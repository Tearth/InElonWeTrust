using System.Net.Http;
using System.Threading.Tasks;

namespace InElonWeTrust.Core.Services.Changelog
{
    public class ChangelogService
    {
        private const string ReadmeUrl = "https://raw.githubusercontent.com/Tearth/InElonWeTrust/master/CHANGELOG.md";

        public async Task<string> GetChangelog()
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetStringAsync(ReadmeUrl);
            }
        }
    }
}
