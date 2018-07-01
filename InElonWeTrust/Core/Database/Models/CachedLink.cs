using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Database.Models
{
    public class CachedLink
    {
        public int ID { get; set; }
        public string Full { get; set; }
        public string Shortcut { get; set; }
    }
}
