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

        private readonly Timer _notificationsUpdateTimer;
        private bool _reloadingCacheState;
        private HttpClient _httpClient;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const string SpaceXProfileId = "130608600@N05";
        private const string ImagesListUrl = "?method=flickr.people.getPhotos&api_key={0}&user_id={1}&per_page=500&page={2}&format=json&nojsoncallback=1";
        private const string ImageSizesUrl = "?method=flickr.photos.getSizes&api_key={0}&photo_id={1}&format=json&nojsoncallback=1";
        private const string ImageDetailsUrl = "?method=flickr.photos.getInfo&api_key={0}&photo_id={1}&format=json&nojsoncallback=1";

        private const int UpdateNotificationsIntervalMinutes = 15;

        public FlickrService()
        {
            _httpClient = new HttpClient {BaseAddress = new Uri("https://www.flickr.com/services/rest")};

            _notificationsUpdateTimer = new Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _notificationsUpdateTimer.Elapsed += NotificationsUpdateTimerOnElapsed;
            _notificationsUpdateTimer.Start();
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
            if (_reloadingCacheState)
            {
                return;
            }

            _reloadingCacheState = true;
            try
            {
                using (var databaseContext = new DatabaseContext())
                {
                    _logger.Info("Reload Flickr cached photos starts");

                    var currentPage = 1;

                    var sendNotifyWhenNewPhoto = databaseContext.CachedFlickrPhotos.Any();
                    var newPhotos = 0;

                    while (true)
                    {
                        var response = await _httpClient.GetStringAsync(string.Format(ImagesListUrl, SettingsLoader.Data.FlickrKey, SpaceXProfileId, currentPage));
                        var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoListResponse>(response);

                        foreach (var photo in parsedResponse.Photos.Photo.AsEnumerable().Reverse())
                        {
                            if (!await databaseContext.CachedFlickrPhotos.AnyAsync(p => p.Id == photo.Id))
                            {
                                var source = await GetImageUrlAsync(photo.Id);
                                var date = await GetImageUploadDateAsync(photo.Id);

                                var cachedPhoto = new CachedFlickrPhoto(photo, date, source);

                                await databaseContext.CachedFlickrPhotos.AddAsync(cachedPhoto);
                                newPhotos++;

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
                    _logger.Info($"Flickr update finished ({newPhotos} sent, {photosCount} photos in database)");
                }
            }
            finally
            {
                _reloadingCacheState = false;
            }
        }

        private async void NotificationsUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
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
            var response = await _httpClient.GetStringAsync(string.Format(ImageSizesUrl, SettingsLoader.Data.FlickrKey, photoId));
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoSizesResponse>(response);

            return parsedResponse.Sizes.Size.Last().Source;
        }

        private async Task<DateTime> GetImageUploadDateAsync(string photoId)
        {
            var response = await _httpClient.GetStringAsync(string.Format(ImageDetailsUrl, SettingsLoader.Data.FlickrKey, photoId));
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoInfoResponse>(response);

            return new DateTime().UnixTimeStampToDateTime(long.Parse(parsedResponse.Photo.DateUploaded));
        }
    }
}
