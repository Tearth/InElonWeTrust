using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Services.Flickr
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
