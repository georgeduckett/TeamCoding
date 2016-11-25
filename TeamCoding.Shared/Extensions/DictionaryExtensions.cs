using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> valueComparer = null)
        { // http://stackoverflow.com/a/3928856
            if (first == second)
                return true;
            if (first == null || second == null)
                return false;
            if (first.Count != second.Count)
                return false;

            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            foreach (var kvp in first)
            {
                if (!second.TryGetValue(kvp.Key, out var secondValue))
                    return false;
                if (!valueComparer.Equals(kvp.Value, secondValue))
                    return false;
            }
            return true;
        }
    }
}
