using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArtifactStore.Helpers
{
    public static class CollectionHelper
    {
        #region ICollection<T> Extension Methods

        public static void AddIfNotListed<T>(this ICollection<T> list, T item)
        {
            if (list.Contains(item))
                return;

            list.Add(item);
        }

        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> range)
        {
            foreach (var item in range)
                list.Add(item);
        }

        public static void AddRangeIfNotListed<T>(this ICollection<T> list, IEnumerable<T> range)
        {
            foreach (var item in range)
                list.AddIfNotListed(item);
        }

        #endregion ICollection<T> Extension Methods

        #region IEnumerable<T> Extension Methods

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> enumeration,
            Func<T, IEnumerable<T>> childSelector)
        {
            return enumeration.SelectMany(element => EnumerableFrom(element)
                .Concat(SelectRecursive(childSelector(element) ?? Enumerable.Empty<T>(), childSelector)));
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
        {
            return new ObservableCollection<T>(list);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
        {
            return new HashSet<T>(list);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }

        public static bool HasMoreThanOneItem<T>(this ICollection<T> list)
        {
            return list.Count > 1;
        }

        public static bool HasMoreThanOneItem<T>(this IEnumerable<T> list)
        {
            return list.Take(2).Count() == 2;
        }

        public static int SequenceCompare<T>(this IEnumerable<T> values, IEnumerable<T> otherValues)
        {
            return SequenceCompare(values, otherValues, Comparer<T>.Default);
        }

        public static int SequenceCompare<T>(this IEnumerable<T> values, IEnumerable<T> otherValues, IComparer<T> comparer)
        {
            // Compare values in each list one-by-one until we find a difference or run out of values in one of the lists.
            int result = values.Zip(otherValues, comparer.Compare).FirstOrDefault(i => i != 0);
            if (result == 0)
            {
                // If we ran out of values, compare the number of values in each list.
                return values.Count().CompareTo(otherValues.Count());
            }
            return result;
        }

        #endregion IEnumerable<T> Extension Methods

        public static IEnumerable<T> EnumerableFrom<T>(T item)
        {
            yield return item;
        }
    }
}