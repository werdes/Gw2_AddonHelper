using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Common.Extensions
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

        public static void AddRange<T>(this HashSet<T> source, IEnumerable<T> list)
        {
            foreach(T item in list)
            {
                if (!source.Contains(item))
                {
                    source.Add(item);
                }
            }
        }

        public static void AddRangeIfNotNull<T>(this List<T> source, IEnumerable<T> values)
        {
            if(values != null)
            {
                values.ForEach(x => source.Add(x));
            }
        }

        public static void InsertIfNotContains<T>(this List<T> source, int index, T item)
        {
            if(!source.Contains(item))
            {
                source.Insert(index, item);
            }
        }
    }
}
