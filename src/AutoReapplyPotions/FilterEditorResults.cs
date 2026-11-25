using System;
using System.Collections.Generic;
using System.Globalization;

namespace AutoReapplyPotions
{
    internal sealed class FilterEditorResults
    {
        internal List<SupportedBuffItemInfo> Catalog { get; } = new List<SupportedBuffItemInfo>();
        internal List<SupportedBuffItemInfo> Selected { get; } = new List<SupportedBuffItemInfo>();

        internal HashSet<SupportedBuffItemCategory> AvailableCategories { get; } =
            new HashSet<SupportedBuffItemCategory>();

        internal void Clear()
        {
            Catalog.Clear();
            Selected.Clear();
            AvailableCategories.Clear();
        }

        internal void Rebuild(
            IEnumerable<SupportedBuffItemInfo> catalog,
            SupportedBuffItemCategory? category,
            string searchText,
            bool selectedTab,
            Func<int, bool> isSelected)
        {
            Clear();
            string search = (searchText ?? string.Empty).Trim();

            foreach (SupportedBuffItemInfo item in catalog)
            {
                if (!MatchesSearch(item, search))
                    continue;

                bool selected = isSelected(item.ItemId);
                if (!selectedTab || selected)
                    AvailableCategories.Add(item.Category);

                if (category.HasValue && item.Category != category.Value)
                    continue;

                Catalog.Add(item);
                if (selected)
                    Selected.Add(item);
            }
        }

        private static bool MatchesSearch(SupportedBuffItemInfo item, string search)
        {
            return search.Length == 0 ||
                   item.ItemName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   item.ItemId.ToString(CultureInfo.InvariantCulture).Contains(search) ||
                   GetCategoryLabel(item.Category).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static string GetCategoryLabel(SupportedBuffItemCategory? category)
        {
            return category switch
            {
                SupportedBuffItemCategory.Potion => "Potions",
                SupportedBuffItemCategory.Flask => "Flasks",
                null => "All",
                _ => category.Value.ToString()
            };
        }
    }
}