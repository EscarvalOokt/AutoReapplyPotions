using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace AutoReapplyPotions
{
    internal static class SupportedBuffItemsCatalog
    {
        private static readonly Dictionary<int, SupportedBuffItemInfo> _itemsById =
            new Dictionary<int, SupportedBuffItemInfo>();

        private static bool _isBuilt;

        public static int Count
        {
            get
            {
                EnsureBuilt();
                return _itemsById.Count;
            }
        }

        public static IReadOnlyCollection<SupportedBuffItemInfo> Items
        {
            get
            {
                EnsureBuilt();
                return _itemsById.Values;
            }
        }

        public static void Build()
        {
            if (_isBuilt)
                return;

            _itemsById.Clear();

            for (var itemId = 1; itemId < ItemID.Count; itemId++)
            {
                try
                {
                    var item = new Item();
                    item.SetDefaults(itemId);

                    if (!AutoReapplyRules.IsSupportedItem(item))
                        continue;

                    string itemName = GetItemDisplayName(itemId, item);
                    SupportedBuffItemCategory category = SupportedBuffItemClassifier.Classify(item);
                    var info = new SupportedBuffItemInfo(itemId, itemName, category);

                    _itemsById[itemId] = info;
                }
                catch (Exception ex)
                {
                    ModLog.Warn($"Failed to inspect item {itemId}: {ex.Message}");
                }
            }

            _isBuilt = true;
            ModLog.Info($"Supported buff items catalog built. Found {_itemsById.Count} item(s).");
        }

        public static void Invalidate()
        {
            _itemsById.Clear();
            _isBuilt = false;
        }

        public static bool ContainsItem(int itemId)
        {
            EnsureBuilt();
            return _itemsById.ContainsKey(itemId);
        }

        public static bool TryGetItem(int itemId, out SupportedBuffItemInfo info)
        {
            EnsureBuilt();
            return _itemsById.TryGetValue(itemId, out info);
        }

        public static List<SupportedBuffItemInfo> GetSortedItems()
        {
            EnsureBuilt();

            return _itemsById.Values.OrderBy(x => x.ItemId).ToList();
        }

        private static void EnsureBuilt()
        {
            if (!_isBuilt)
                Build();
        }

        private static string GetItemDisplayName(int itemId, Item item)
        {
            string itemName = null;

            try
            {
                itemName = Lang.GetItemNameValue(itemId);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // Best-effort localization lookup; fall back to item.Name below.
            }

            bool shouldUseItemName = string.IsNullOrWhiteSpace(itemName) && item != null;
            if (shouldUseItemName)
                itemName = item.Name;

            if (string.IsNullOrWhiteSpace(itemName))
                itemName = $"Item_{itemId}";

            return itemName;
        }
    }
}