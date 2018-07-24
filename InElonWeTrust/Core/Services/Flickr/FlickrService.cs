using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr.PhotoInfo;
using InElonWeTrust.Core.Services.Flickr.PhotosList;
using InElonWeTrust.Core.Services.Flickr.PhotoSizes;
using InElonWeTrust.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.Flickr
{
    public class FlickrService
    {
        public event EventHandler<CachedFlickrPhoto> OnNewFlickrPhoto;

        private readonly Timer _imageRangesUpdateTimer;
        private readonly object _reloadingCacheLock;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const string SpaceXProfileId = "130608600@N05";
        private const string ImagesListUrl = "https://www.flickr.com/services/rest?method=flickr.people.getPhotos&api_key={0}&user_id={1}&per_page=500&page={2}&format=json&nojsoncallback=1";
        private const string ImageSizesUrl = "https://www.flickr.com/services/rest?method=flickr.photos.getSizes&api_key={0}&photo_id={1}&format=json&nojsoncallback=1";
        private const string ImageDetailsUrl = "https://www.flickr.com/services/rest?method=flickr.photos.getInfo&api_key={0}&photo_id={1}&format=json&nojsoncallback=1";

        private const int UpdateNotificationsIntervalMinutes = 15;

        public FlickrService()
        {
            _imageRangesUpdateTimer = new Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _imageRangesUpdateTimer.Elapsed += TweetRangesUpdateTimer_Elapsed;
            _imageRangesUpdateTimer.Start();

            _reloadingCacheLock = new object();
        }

        public async Task<CachedFlickrPhoto> GetRandomPhotoAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return await databaseContext.CachedFlickrPhotos.OrderBy(r => Guid.NewGuid()).FirstAsync();
            }
        }

        public async Task ReloadCachedPhotosAsync()
        {
            if (!System.Threading.Monitor.TryEnter(_reloadingCacheLock))
            {
                return;
            }

            try
            {
                using (var databaseContext = new DatabaseContext())
                using (var httpClient = new HttpClient())
                {
                    _logger.Info("Reload Flickr cached photos starts");

                    var currentPage = 1;
                    var sendNotifyWhenNewPhoto = databaseContext.CachedFlickrPhotos.Any();

                    while (true)
                    {
                        var response = await httpClient.GetStringAsync(string.Format(ImagesListUrl, SettingsLoader.Data.FlickrKey, SpaceXProfileId, currentPage));
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

                        _logger.Info($"Flickr page ({currentPage}) done");

                        if (currentPage >= parsedResponse.Photos.Pages)
                        {
                            break;
                        }

                        currentPage++;
                    }

                    await databaseContext.SaveChangesAsync();

                    var photosCount = await databaseContext.CachedFlickrPhotos.CountAsync();
                    _logger.Info($"Flickr download finished ({photosCount} photos downloaded)");
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(_reloadingCacheLock);
            }
        }

        private async void TweetRangesUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await ReloadCachedPhotosAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to reload cached photos");
            }
        }

        private async Task<string> GetImageUrlAsync(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync(string.Format(ImageSizesUrl, SettingsLoader.Data.FlickrKey, photoId));
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoSizesResponse>(response);

            return parsedResponse.Sizes.Size.First(p => p.Label == "Original").Source;
        }

        private async Task<DateTime> GetImageUploadDateAsync(string photoId)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync(string.Format(ImageDetailsUrl, SettingsLoader.Data.FlickrKey, photoId));
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoInfoResponse>(response);

            return new DateTime().UnixTimeStampToDateTime(long.Parse(parsedResponse.Photo.DateUploaded));
        }
    }
}
