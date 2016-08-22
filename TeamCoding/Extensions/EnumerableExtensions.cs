using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> TakeWhileInclusive<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        { // http://stackoverflow.com/a/3098714
            var foundMatch = false;
            foreach (T item in source)
            {
                if (predicate(item))
                {
                    foundMatch = true;
                    yield return item;
                }
                else
                {
                    if (foundMatch)
                    {
                        yield return item;
                    }
                    yield break;
                }
            }
        }
    }
}
