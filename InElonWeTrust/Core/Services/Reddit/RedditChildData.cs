using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.Reddit
{
    public class RedditChildData
    {
        public string Name { get; set; }
        public string Title { get; set; }

        [JsonProperty("ups")]
        public int Upvotes { get; set; }

        public string Thumbnail { get; set; }
        public float Created { get; set; }
        public string Author { get; set; }
        public string Permalink { get; set; }
    }
}
