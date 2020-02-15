using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers.Extensions;
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
        public event EventHandler<List<CachedFlickrPhoto>> OnNewFlickrPhoto;

        private readonly System.Timers.Timer _notificationsUpdateTimer;
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);
        private readonly HttpClient _httpClient;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const string SpaceXProfileId = "130608600@N05";
        private const string ImagesListUrl = "?method=flickr.people.getPhotos&api_key={0}&user_id={1}&per_page=500&page={2}&format=json&nojsoncallback=1";
        private const string ImageSizesUrl = "?method=flickr.photos.getSizes&api_key={0}&photo_id={1}&format=json&nojsoncallback=1";
        private const string ImageDetailsUrl = "?method=flickr.photos.getInfo&api_key={0}&photo_id={1}&format=json&nojsoncallback=1";

        private const int UpdateNotificationsIntervalMinutes = 15;

        public FlickrService()
        {
            _httpClient = new HttpClient {BaseAddress = new Uri("https://www.flickr.com/services/rest")};

            _notificationsUpdateTimer = new System.Timers.Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _notificationsUpdateTimer.Elapsed += NotificationsUpdateTimerOnElapsedAsync;
            _notificationsUpdateTimer.Start();
        }

        public CachedFlickrPhoto GetRandomPhotoAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return databaseContext.CachedFlickrPhotos.RandomRow("CachedFlickrPhotos").First();
            }
        }

        public async Task ReloadCachedPhotosAsync()
        {
            if (!await _updateSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                using (var databaseContext = new DatabaseContext())
                {
                    _logger.Info("Reload Flickr cached photos starts");
                    
                    var sendNotifyWhenNewPhoto = await databaseContext.CachedFlickrPhotos.AnyAsync();
                    var allPhotos = await GetListOfPhotosAsync();
                    var photosToSend = GetNewPhotosToSend(allPhotos);
                    var cachedPhotosToSend = await AddPhotosToDatabaseAsync(photosToSend);

                    if (cachedPhotosToSend.Count > 0 && sendNotifyWhenNewPhoto)
                    {
                        OnNewFlickrPhoto?.Invoke(this, cachedPhotosToSend);
                    }

                    await databaseContext.SaveChangesAsync();

                    var photosCount = await databaseContext.CachedFlickrPhotos.CountAsync();
                    _logger.Info($"Flickr update finished ({photosToSend.Count} sent, {photosCount} photos in database)");
                }
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async void NotificationsUpdateTimerOnElapsedAsync(object sender, ElapsedEventArgs e)
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
            var response = await _httpClient.GetStringWithRetriesAsync(string.Format(ImageSizesUrl, SettingsLoader.Data.FlickrKey, photoId));
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoSizesResponse>(response);

            return parsedResponse.Sizes.Size.Last().Source;
        }

        private async Task<DateTime> GetImageUploadDateAsync(string photoId)
        {
            var response = await _httpClient.GetStringWithRetriesAsync(string.Format(ImageDetailsUrl, SettingsLoader.Data.FlickrKey, photoId));
            var parsedResponse = JsonConvert.DeserializeObject<FlickrPhotoInfoResponse>(response);

            return new DateTime().UnixTimeStampToDateTime(long.Parse(parsedResponse.Photo.DateUploaded));
        }

        private async Task<List<FlickrPhoto>> GetListOfPhotosAsync()
        {
            var page = 1;
            var photos = new List<FlickrPhoto>();

            while (true)
            {
                var response = await _httpClient.GetStringWithRetriesAsync(string.Format(ImagesListUrl, SettingsLoader.Data.FlickrKey, SpaceXProfileId, page));
                var responseContainer =  JsonConvert.DeserializeObject<FlickrPhotoListResponse>(response);
                photos.AddRange(responseContainer.Photos.Photo);

                if (page >= responseContainer.Photos.Pages)
                {
                    break;
                }

                page++;
            }

            photos.Reverse();
            return photos;
        }

        private List<FlickrPhoto> GetNewPhotosToSend(List<FlickrPhoto> allPhotos)
        {
            using (var databaseContext = new DatabaseContext())
            {
                return allPhotos.Where(newPhoto => !databaseContext.CachedFlickrPhotos.Any(photoInDb => photoInDb.Id == newPhoto.Id)).ToList();
            }
        }

        private async Task<List<CachedFlickrPhoto>> AddPhotosToDatabaseAsync(List<FlickrPhoto> photos)
        {
            var cachedPhotos = new List<CachedFlickrPhoto>();
            using (var databaseContext = new DatabaseContext())
            {
                foreach (var photo in photos)
                {
                    var source = await GetImageUrlAsync(photo.Id);
                    var date = await GetImageUploadDateAsync(photo.Id);
                    var cachedPhoto = new CachedFlickrPhoto(photo, date, source);

                    await databaseContext.CachedFlickrPhotos.AddAsync(cachedPhoto);
                    cachedPhotos.Add(cachedPhoto);
                }

                await databaseContext.SaveChangesAsync();
            }

            return cachedPhotos;
        }
    }
}
