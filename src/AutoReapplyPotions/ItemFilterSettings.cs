using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace AutoReapplyPotions
{
    internal static class ItemFilterSettings
    {
        private static readonly HashSet<int> _filteredItemIds = new HashSet<int>();

        public static ItemFilterMode Mode { get; private set; } = ItemFilterMode.Disabled;
        public static IReadOnlyCollection<int> FilteredItemIds => _filteredItemIds;

        public static void Update(ItemFilterMode mode, IEnumerable<int> itemIds)
        {
            Mode = mode;
            _filteredItemIds.Clear();

            if (itemIds == null)
                return;

            foreach (int itemId in itemIds)
            {
                if (itemId <= 0)
                    continue;

                if (!IsPotentiallySupportedItemId(itemId))
                    continue;

                _filteredItemIds.Add(itemId);
            }
        }

        public static bool Contains(int itemId)
        {
            return _filteredItemIds.Contains(itemId);
        }

        public static bool Add(int itemId)
        {
            if (itemId <= 0)
                return false;

            if (!IsPotentiallySupportedItemId(itemId))
                return false;

            return _filteredItemIds.Add(itemId);
        }

        public static bool Remove(int itemId)
        {
            return _filteredItemIds.Remove(itemId);
        }

        public static bool Toggle(int itemId)
        {
            if (Contains(itemId))
            {
                _filteredItemIds.Remove(itemId);
                return false;
            }

            bool canAddItem = itemId > 0 && IsPotentiallySupportedItemId(itemId);
            if (canAddItem)
            {
                _filteredItemIds.Add(itemId);
                return true;
            }

            return false;
        }

        public static void Clear()
        {
            _filteredItemIds.Clear();
        }

        public static List<int> GetSortedIds()
        {
            return _filteredItemIds.OrderBy(x => x).ToList();
        }

        public static string SerializeToConfigString()
        {
            return string.Join(",", GetSortedIds());
        }

        public static bool IsItemAllowed(int itemId)
        {
            if (Mode == ItemFilterMode.Disabled)
                return true;

            bool contains = _filteredItemIds.Contains(itemId);

            if (Mode == ItemFilterMode.Blacklist)
                return !contains;

            return contains;
        }

        public static List<int> ParseItemIds(string raw)
        {
            var result = new List<int>();

            if (string.IsNullOrWhiteSpace(raw))
                return result;

            char[] separators = [',', ';', '\n', '\r', '\t', ' '];
            string[] parts = raw.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                bool isValidItemId = int.TryParse(part, out int itemId) && itemId > 0;
                if (isValidItemId)
                    result.Add(itemId);
            }

            return result;
        }

        private static bool IsPotentiallySupportedItemId(int itemId)
        {
            try
            {
                var item = new Item();
                item.SetDefaults(itemId);

                return AutoReapplyRules.IsSupportedItem(item);
            }
            catch
            {
                return false;
            }
        }
    }
}