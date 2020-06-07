Addusing System;
using System.Collections.Generic;

namespace InElonWeTrust.Core.Services.Sn
{
    public class SnService
    {
        private Random _random;
        private List<(string Name, string Verb)> _parts;
        private readonly string _template = "That’s a shame SN{0} has RUD’d, but {2} {3} no doubt " +
                                            "been redesigned anyway, and I’m sure SN{1} will be along in a " +
                                            "matter of days! I have a good feeling SN{1} is the one that will " +
                                            "make the hop, no doubt in just a couple of weeks!" + 
                                            "\n\n" +
                                            "This is actually a good thing!! More data!! If you’re not blowing " +
                                            "things up you’re not innovating fast enough!!";

        public SnService()
        {
            _random = new Random();
            _parts = new List<(string Name, string Verb)>
            {
                ("raptor", "has"),
                ("bulkhead", "has"),
                ("COPV", "has"),
                ("legs", "have"),
                ("welds", "have"),
                ("test stand", "has"),
                ("pipes", "have")
            };
        }

        public string GetSnText(int? snNumber)
        {
            var number = snNumber;
            if (number == null)
            {
                number = _random.Next(1, 1000);
            }

            var part = _parts[_random.Next(0, _parts.Count - 1)];
            return string.Format(_template, number, number + 1, part.Name, part.Verb);
        }
    }
}