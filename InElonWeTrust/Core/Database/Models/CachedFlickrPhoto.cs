using System;
using InElonWeTrust.Core.Services.Flickr.PhotosList;

namespace InElonWeTrust.Core.Database.Models
{
    public class CachedFlickrPhoto
    {
        public string Id { get; set; }
        public DateTime UploadDate { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }

        public CachedFlickrPhoto()
        {

        }

        public CachedFlickrPhoto(FlickrPhoto photo, DateTime uploadDate, string source)
        {
            Id = photo.Id;
            UploadDate = uploadDate;
            Source = source;
            Title = photo.Title;
        }
    }
}
