namespace StreamerBotLib.Static
{
    public static class Extensions
    {
        public static DateTime Max(DateTime A, DateTime B) => A <= B ? B : A;


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
        /// Uniquely adds an item to the provided <paramref name="List"/>, first checking if <paramref name="List"/> contains the item.
        /// </summary>
        /// <typeparam name="T">The generic type of the list.</typeparam>
        /// <param name="List">The List to add the item.</param>
        /// <param name="Item">The item to check if List contains and then add this item.</param>
        /// <param name="comparer">An item comparer to use to determine if two list objects are equal.</param>
        /// <returns><code>true</code> - if added to List, <code>false</code> - if List contained item and not added.</returns>
        public static bool UniqueAdd<T>(this List<T> List, T Item, IEqualityComparer<T> comparer)
        {
            bool found = false;

            if (!List.Where(Listitem => comparer.Equals(Item, Listitem)).Select(Listitem => new { }).Any())
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
            foreach (T item in ItemEnumerable)
            {
                UniqueAdd(List, item);
            }
        }

        /// <summary>
        /// Uniquely adds an item to the provided <paramref name="ICollection"/>, first checking if <paramref name="ICollection"/> contains the item.
        /// </summary>
        /// <typeparam name="T">The generic type of the ICollection.</typeparam>
        /// <param name="ICollection">The ICollection to add the item.</param>
        /// <param name="Item">The item to check if ICollection contains and then add this item.</param>
        /// <returns><code>true</code> - if added to ICollection, <code>false</code> - if ICollection contained item and not added.</returns>
        public static bool UniqueAdd<T>(this ICollection<T> ICollection, T Item)
        {
            bool found = false;
            if (!ICollection.Contains(Item))
            {
                ICollection.Add(Item);
                found = true;
            }

            return found;
        }

        /// <summary>
        /// Uniquely adds an item to the provided <paramref name="ICollection"/>, first checking if <paramref name="ICollection"/> contains the item.
        /// </summary>
        /// <typeparam name="T">The generic type of the ICollection.</typeparam>
        /// <param name="ICollection">The ICollection to add the item.</param>
        /// <param name="Item">The item to check if ICollection contains and then add this item.</param>
        /// <param name="comparer">An item comparer to use to determine if two ICollection objects are equal.</param>
        /// <returns><code>true</code> - if added to ICollection, <code>false</code> - if ICollection contained item and not added.</returns>
        public static bool UniqueAdd<T>(this ICollection<T> ICollection, T Item, IEqualityComparer<T> comparer)
        {
            bool found = false;

            if (!ICollection.Where(ICollectionitem => comparer.Equals(Item, ICollectionitem)).Select(ICollectionitem => new { }).Any())
            {
                ICollection.Add(Item);
                found = true;
            }

            return found;
        }

        /// <summary>
        /// Uniquely adds a group of items to the provided <paramref name="ICollection"/>, first checking if each item is in the <paramref name="ICollection"/>.
        /// </summary>
        /// <typeparam name="T">The ICollection item type.</typeparam>
        /// <param name="ICollection">The ICollection to add the new item.</param>
        /// <param name="ItemEnumerable">The group of items to add.</param>
        public static void UniqueAddRange<T>(this ICollection<T> ICollection, IEnumerable<T> ItemEnumerable)
        {
            foreach (T item in ItemEnumerable)
            {
                UniqueAdd(ICollection, item);
            }
        }
    }
}
