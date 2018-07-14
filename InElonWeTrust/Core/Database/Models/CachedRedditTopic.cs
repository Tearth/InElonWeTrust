using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Database.Models
{
    public class CachedRedditTopic
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CachedRedditTopic()
        {

        }

        public CachedRedditTopic(string name)
        {
            Name = name;
        }
    }
}
