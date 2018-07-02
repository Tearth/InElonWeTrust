using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InElonWeTrust.Core.Helpers
{
    public static class Extensions
    {
        private static Random _random;

        static Extensions()
        {
            _random = new Random();
        }

        public static T GetRandomItem<T>(this List<T> enumerable)
        {
            var index = _random.Next(0, enumerable.Count() - 1);
            return enumerable[index];
        }
    }
}
