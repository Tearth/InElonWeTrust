using System.Net.Http;
using System.Threading.Tasks;

namespace InElonWeTrust.Core.Services.Changelog
{
    public class ChangelogService
    {
        public async Task<string> GetChangelog()
        {
            var httpClient = new HttpClient();
            return await httpClient.GetStringAsync("https://raw.githubusercontent.com/Tearth/InElonWeTrust/master/CHANGELOG.md");
        }
    }
}
