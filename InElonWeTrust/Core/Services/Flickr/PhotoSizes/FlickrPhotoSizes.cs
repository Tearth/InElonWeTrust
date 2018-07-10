using System.Collections.Generic;

namespace InElonWeTrust.Core.Services.Flickr.PhotoSizes
{
    public class FlickrPhotoSizes
    {
        public bool CanBlog { get; set; }
        public bool CanPrint { get; set; }
        public bool CanDownload { get; set; }
        public List<FlickrPhotoSize> Size { get; set; }
    }
}
