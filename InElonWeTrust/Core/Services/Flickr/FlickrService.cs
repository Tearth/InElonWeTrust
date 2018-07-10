using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
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
        private Timer _imageRangesUpdateTimer;

        private const string SpaceXProfileId = "130608600@N05";
        private const int IntervalMinutes = 30;

        public FlickrService()
        {
            _imageRangesUpdateTimer = new Timer(IntervalMinutes * 60 * 1000);
            _imageRangesUpdateTimer.Elapsed += TweetRangesUpdateTimer_Elapsed;

            _imageRangesUpdateTimer.Start();
        }

        public async Task<CachedFlickrPhoto> GetRandomPhoto()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return databaseContext.CachedFlickrPhotos.OrderBy(r => Guid.NewGuid()).First();
            }
        }

        public async Task ReloadCachedPhotos()
        {
            var httpClient = new HttpClient();

            using (var databaseContext = new DatabaseContext())
            {
                Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, "Flickr download start", DateTime.Now);

                var currentPage = 1;
                while (true)
                {
                    var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.people.getPhotos&api_key={SettingsLoader.Data.FlickrKey}&user_id={SpaceXProfileId}&per_page=500&page={currentPage}&format=json&nojsoncallback=1");
                    var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoListResponse>(response);

                    foreach (var photo in parsedResponse.Photos.Photo)
                    {
                        if (!databaseContext.CachedFlickrPhotos.Any(p => p.ID == photo.Id))
                        {
                            var source = await GetImageUrl(photo.Id);
                            var date = await GetImageUploadDate(photo.Id);

                            var cachedPhoto = new CachedFlickrPhoto(photo, date, source);
                            databaseContext.CachedFlickrPhotos.Add(cachedPhoto);
                        }

                    }

                    if (currentPage == parsedResponse.Photos.Pages)
                    {
                        break;
                    }

                    Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Flickr page ({currentPage}) done", DateTime.Now);

                    currentPage++;
                }

                databaseContext.SaveChanges();

                var photosCount = databaseContext.CachedFlickrPhotos.Count();
                Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Flickr download finished ({photosCount} photos downloaded)", DateTime.Now);
            }
        }

        private void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReloadCachedPhotos();
        }

        private async Task<string> GetImageUrl(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.photos.getSizes&api_key={SettingsLoader.Data.FlickrKey}&photo_id={photoId}&format=json&nojsoncallback=1");
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoSizesResponse>(response);

            return parsedResponse.Sizes.Size.First(p => p.Label == "Original").Source;
        }

        private async Task<DateTime> GetImageUploadDate(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.photos.getInfo&api_key={SettingsLoader.Data.FlickrKey}&photo_id={photoId}&format=json&nojsoncallback=1");
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoInfoResponse>(response);

            return new DateTime().UnixTimeStampToDateTime(long.Parse(parsedResponse.Photo.DateUploaded));
        }
    }
}
