using System;
using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Flickr;
using Microsoft.EntityFrameworkCore.Storage;

namespace InElonWeTrust.Core.Database.Models
{
    public class CachedFlickrPhoto
    {
        public string ID { get; set; }
        public DateTime UploadDate { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }

        public CachedFlickrPhoto()
        {

        }

        public CachedFlickrPhoto(FlickrPhoto photo, DateTime uploadDate, string source)
        {
            ID = photo.Id;
            UploadDate = uploadDate;
            Source = source;
            Title = photo.Title;
        }
    }
}
