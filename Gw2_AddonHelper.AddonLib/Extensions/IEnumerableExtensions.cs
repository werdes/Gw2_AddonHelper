﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T TryGet<T>(this IEnumerable<T> collection, int index)
        {
            if (collection.Count() > index) return collection.ElementAt(index);
            return default(T);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> list)
        {
            return list.Select(x => source.Contains(x)).Any(x => x == true);
        }
    }
}
