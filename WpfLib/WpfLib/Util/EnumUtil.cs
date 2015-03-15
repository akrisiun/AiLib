using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ai.Util
{

    // Action is a delegate (pointer) to a method, that takes zero, one or more input parameters, but does not return anything.
    // Action is more commonly used for things like List<T>.ForEach
    // Action - When you want a delegate for a function that may or may not take parameters
    //         often used for anonymous event handlers: .Click += (sender, e) => {}

    // Func is a delegate (pointer) to a method, that takes zero, one or more input parameters, and returns a value (or refence).

    // Predicate is a special kind of Func often used for comparisons 
    // Predicate is just a special cased Func<T, bool> really, introduced before all of the Func
    // Predicate is mostly used in List<T> for methods like FindAll and RemoveAll

    // ObservableCollection is a generic dynamic data collection that provides notifications (using an interface "INotifyCollectionChanged")

    public static class EnumUtil
    {
        public static IEnumerable<TSource> Distinct<TSource>(IEnumerable<TSource> source) // , IEqualityComparer<TSource> comparer)
        {
            Collection<TSource> set = new Collection<TSource>();

            foreach (TSource element in source)
            {
                if (set.Contains(element))
                    continue;

                set.Add(element);
                yield return element;
            }
        }

        public static IEnumerator<T> Match<T>(this ReadOnlyCollection<T> source, Func<T, bool> match)
        {
            foreach (T obj in source)
                if (match(obj))
                    yield return obj;

            yield break;
        }

        // Clone copy of source, if source collection is changeing
        public static Collection<T> CloneMatch<T>(this ICollection<T> source, Func<T, bool> match)
        {
            var filter = new Collection<T>();

            foreach (T obj in source)
                if (match(obj))
                    filter.Add(obj);

            return filter;
        }

        // System.Linq.IOrderedQueryable<T>
        public static Collection<TSource>
               OrderBy<TSource>(this ICollection<TSource> source, Func<TSource, TSource, bool> IsBigger)
        {
            var sorted = new Collection<TSource>();
            TSource last = default(TSource);

            foreach (TSource obj in source)
            {
                if (sorted.Count == 0 || IsBigger(obj, last))
                    sorted.Add(obj);
                else
                {
                    foreach (TSource obj2 in sorted)
                        if (IsBigger(obj2, obj))
                        {
                            int idx = sorted.IndexOf(obj2);
                            sorted.Insert(0, obj);
                            break;
                        }
                }
                last = obj;
            }
            return sorted;
        }

        // CastIterator
        public static IEnumerable<TResult> CastSource<TResult>(System.Collections.IEnumerable source)
        {
            foreach (object obj in source)
                yield return (TResult)obj;
        }

        public static T FindItem<T>(this IEnumerable<T> source, Func<T, bool> match) where T : class
        {
            foreach (T obj in source)
                if (match(obj))
                    return obj;

            return null;
        }

        public static T? FindItemValue<T>(this IEnumerable<T> source, Func<T, bool> match) where T : struct
        {
            foreach (T obj in source)
                if (match(obj))
                    return obj;

            return null;
        }

        public static int NullCount<T>(this ICollection<T> list)
        {
            return list == null ? 0 : list.Count;
        }

    }

}
