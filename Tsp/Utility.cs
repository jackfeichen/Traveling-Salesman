using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Radex.Tsp
{
    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 one, T2 two)
        {
            var result = new Tuple<T1, T2>(one, two);
            return result;
        }
    }

    [DataContract]
    public class Tuple<T1, T2>
    {
        public T1 One { get; set; }
        public T2 Two { get; set; }

        public Tuple() { }
        public Tuple(T1 one, T2 two)
        {
            One = one;
            Two = two;
        }
    }

    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> col, Action<T> act)
        {
            foreach (var item in col)
                act(item);
        }

        public static void ForEach<T>(this IList<T> list, Action<T> act)
        {
            foreach (var item in list)
                act(item);
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            // NOTE: yield actually does not enumerate thorough, as the OrderBy requires the list
            //  to be fully explored prior to returning the result.
            foreach(var item in source.OrderBy(s => Guid.NewGuid().GetHashCode()))
                yield return item;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            var elements = source.ToArray();
            // Note: i > 0 to avoid final pointless iteration 
            for (var i = elements.Length - 1; i > 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself) 
                var swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
                // we don't actually perform the swap, we can forget about the 
                // swapped element because we already returned it. 
            }

            // there is one item remaining that was not returned - we return it now 
            yield return elements[0];
        } 
    }

    public static class XLinqExtensions
    {
        public static string GetAttr(this XElement element, XName attr, string defaultValue)
        {
            return element.Attribute(attr) != null ?
                       element.Attribute(attr).Value
                       : defaultValue;
        }
    }
}

