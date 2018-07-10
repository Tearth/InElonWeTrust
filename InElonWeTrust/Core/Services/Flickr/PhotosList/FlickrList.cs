using System.Collections.Generic;

namespace InElonWeTrust.Core.Services.Flickr.PhotosList
{
    public class FlickrList
    {
        public int Page { get; set; }
        public int Pages { get; set; }
        public int PerPage { get; set; }
        public int Total { get; set; }
        public List<FlickrPhoto> Photo { get; set; }
    }
}
