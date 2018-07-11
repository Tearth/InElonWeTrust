using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr.PhotoInfo;
using InElonWeTrust.Core.Services.Flickr.PhotosList;
using InElonWeTrust.Core.Services.Flickr.PhotoSizes;
using InElonWeTrust.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace InElonWeTrust.Core.Services.Flickr
{
    public class FlickrService
    {
        public event EventHandler<CachedFlickrPhoto> OnNewFlickrPhoto;

        private Timer _imageRangesUpdateTimer;
        private bool _reloadingCache;

        private const string SpaceXProfileId = "130608600@N05";
        private const int IntervalMinutes = 1;

        public FlickrService()
        {
            _imageRangesUpdateTimer = new Timer(IntervalMinutes * 60 * 1000);
            _imageRangesUpdateTimer.Elapsed += TweetRangesUpdateTimer_Elapsed;
            _imageRangesUpdateTimer.Start();
        }

        public async Task<CachedFlickrPhoto> GetRandomPhotoAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return await databaseContext.CachedFlickrPhotos.OrderBy(r => Guid.NewGuid()).FirstAsync();
            }
        }

        public async Task ReloadCachedPhotosAsync(bool sendNotifyWhenNewPhoto)
        {
            if (_reloadingCache)
            {
                return;
            }

            _reloadingCache = true;

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
                        if (!await databaseContext.CachedFlickrPhotos.AnyAsync(p => p.Id == photo.Id))
                        {
                            var source = await GetImageUrlAsync(photo.Id);
                            var date = await GetImageUploadDateAsync(photo.Id);

                            var cachedPhoto = new CachedFlickrPhoto(photo, date, source);
                            await databaseContext.CachedFlickrPhotos.AddAsync(cachedPhoto);

                            if (sendNotifyWhenNewPhoto)
                            {
                                OnNewFlickrPhoto?.Invoke(this, cachedPhoto);
                            }
                        }

                    }

                    if (currentPage >= parsedResponse.Photos.Pages)
                    {
                        break;
                    }

                    Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Flickr page ({currentPage}) done", DateTime.Now);

                    currentPage++;
                }

                await databaseContext.SaveChangesAsync();

                var photosCount = await databaseContext.CachedFlickrPhotos.CountAsync();
                Bot.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"Flickr download finished ({photosCount} photos downloaded)", DateTime.Now);
            }

            _reloadingCache = false;
        }

        private async void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await ReloadCachedPhotosAsync(true);
        }

        private async Task<string> GetImageUrlAsync(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.photos.getSizes&api_key={SettingsLoader.Data.FlickrKey}&photo_id={photoId}&format=json&nojsoncallback=1");
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoSizesResponse>(response);

            return parsedResponse.Sizes.Size.First(p => p.Label == "Original").Source;
        }

        private async Task<DateTime> GetImageUploadDateAsync(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync($"https://www.flickr.com/services/rest?method=flickr.photos.getInfo&api_key={SettingsLoader.Data.FlickrKey}&photo_id={photoId}&format=json&nojsoncallback=1");
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoInfoResponse>(response);

            return new DateTime().UnixTimeStampToDateTime(long.Parse(parsedResponse.Photo.DateUploaded));
        }
    }
}
