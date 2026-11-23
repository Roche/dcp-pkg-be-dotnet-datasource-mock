using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Extensions
{
    public static class CollectionExtensions
    {
        public static int GetCircularIndex<T>(this ICollection<T> collection, uint index)
        {
            var result = (int)(index % collection.Count);
            return result;
        }

        public static T GetCircularItem<T>(this ICollection<T> collection, uint index)
        {
            var circularIndex = collection.GetCircularIndex(index);
            var item = collection.ElementAt(circularIndex);
            return item;
        }

        /// <summary>
        /// Extension method to distinct by specific property.
        /// </summary>
        /// <param name="keySelector">Function to pass object for distinct to work.</param>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}