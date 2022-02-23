using System;
using System.Collections.Generic;

namespace StreamerBotLib.Static
{
    public static class ExtensionList
    {
        /// <summary>
        /// Uniquely adds an item to the provided <paramref name="List"/>, first checking if <paramref name="List"/> contains the item.
        /// </summary>
        /// <typeparam name="T">The generic type of the list.</typeparam>
        /// <param name="List">The List to add the item.</param>
        /// <param name="Item">The item to check if List contains and then add this item.</param>
        /// <returns><code>true</code> - if added to List, <code>false</code> - if List contained item and not added.</returns>
        public static bool UniqueAdd<T>(this List<T> List, T Item)
        {
            bool found = false;
            if (!List.Contains(Item))
            {
                List.Add(Item);
                found = true;
            }

            return found;
        }

        /// <summary>
        /// Uniquely adds a group of items to the provided <paramref name="List"/>, first checking if each item is in the <paramref name="List"/>.
        /// </summary>
        /// <typeparam name="T">The List item type.</typeparam>
        /// <param name="List">The List to add the new item.</param>
        /// <param name="ItemEnumerable">The group of items to add.</param>
        public static void UniqueAddRange<T>(this List<T> List, IEnumerable<T> ItemEnumerable)
        {
            foreach(T item in ItemEnumerable)
            {
                UniqueAdd(List, item);
            }
        }
    }
}
