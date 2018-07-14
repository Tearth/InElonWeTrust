using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.Reddit
{
    public class RedditService
    {
        public async Task<RedditChildData> GetRandomTopic()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://www.reddit.com/r/spacex/random/.json");

            var parsedResponse = JsonConvert.DeserializeObject<List<RedditResponse>>(response);
            return parsedResponse.First().Data.Children.First().Data;
        }
    }
}
