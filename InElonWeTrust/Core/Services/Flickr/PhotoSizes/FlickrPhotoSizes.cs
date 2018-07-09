using System;
using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Flickr.PhotoSizes;

namespace InElonWeTrust.Core.Services.Flickr
{
    public class FlickrPhotoSizes
    {
        public bool CanBlog { get; set; }
        public bool CanPrint { get; set; }
        public bool CanDownload { get; set; }
        public List<FlickrPhotoSize> Size { get; set; }
    }
}
