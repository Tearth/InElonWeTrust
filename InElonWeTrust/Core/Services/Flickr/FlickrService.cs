using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr.PhotoInfo;
using InElonWeTrust.Core.Settings;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace InElonWeTrust.Core.Services.Flickr
{
    public class FlickrService
    {
        private List<FlickrPhoto> _photos;
        private Timer _imageRangesUpdateTimer;

        private const string SpaceXProfileId = "130608600@N05";
        private const int InitialIntervalSeconds = 2;
        private const int IntervalMinutes = 60 * 12;

        public FlickrService()
        {
            _photos = new List<FlickrPhoto>();

            _imageRangesUpdateTimer = new Timer(InitialIntervalSeconds);
            _imageRangesUpdateTimer.Elapsed += TweetRangesUpdateTimer_Elapsed;

            _imageRangesUpdateTimer.Start();
        }

        public async Task<FlickrPhoto> GetRandomPhoto()
        {
            var randomPhoto = _photos.GetRandomItem();
            randomPhoto.Source = await GetImageUrl(randomPhoto.Id);
            randomPhoto.UploadDate = await GetImageUploadDate(randomPhoto.Id);

            return randomPhoto;
        }

        private void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _imageRangesUpdateTimer.Interval = IntervalMinutes * 60 * 1000;
            UpdateTweetRanges();
        }

        private async Task UpdateTweetRanges()
        {
            var httpClient = new HttpClient();
            _photos.Clear();

            var currentPage = 1;
            while (true)
            {
                var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.people.getPhotos&api_key={SettingsLoader.Data.FlickrKey}&user_id={SpaceXProfileId}&per_page=500&page={currentPage}&format=json&nojsoncallback=1");
                var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoListResponse>(response);

                _photos.AddRange(parsedResponse.Photos.Photo);

                if (currentPage == parsedResponse.Photos.Pages)
                {
                    break;
                }

                currentPage++;
            }
        }

        private async Task<string> GetImageUrl(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.photos.getSizes&api_key={SettingsLoader.Data.FlickrKey}&photo_id={photoId}&format=json&nojsoncallback=1");
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoSizesResponse>(response);

            return parsedResponse.Sizes.Size.First(p => p.Label == "Original").Source;
        }

        private async Task<string> GetImageUploadDate(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.photos.getInfo&api_key={SettingsLoader.Data.FlickrKey}&photo_id={photoId}&format=json&nojsoncallback=1");
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoInfoResponse>(response);

            return new DateTime().UnixTimeStampToDateTime(long.Parse(parsedResponse.Photo.DateUploaded)).ToString();
        }
    }
}
