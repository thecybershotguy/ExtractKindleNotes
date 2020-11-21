using System.Collections.Generic;
using System.Linq;

namespace ProjectTools
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Fuses the specified connector.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="connector">The connector.</param>
        /// <returns></returns>
        public static string Fuse<T>(this IEnumerable<T> items, string connector = "; ")
        {
            if (items == null)
                return null;
            if (items.Any() == false)
                return null;
            return string.Join(connector, items);
        }
    }
}
