using System;
using System.Collections.Generic;
using System.Linq;
using TerrariaModder.Core;
using TerrariaModder.Core.UI;
using TerrariaModder.Core.UI.Widgets;

namespace AutoReapplyPotions
{
    internal sealed class FilterUiController
    {
        private const int PanelWidth = 800;
        private const int PanelHeight = 560;

        private const int RowHeight = 28;
        private const int SectionSpacing = 6;
        private const int ControlHeight = 28;
        private const int MinListHeight = 80;

        private readonly ScrollView _catalogScroll = new ScrollView();
        private readonly ScrollView _selectedScroll = new ScrollView();

        private readonly List<CatalogCategoryFilter> _visibleCategoryTabs = new List<CatalogCategoryFilter>();
        private CatalogCategoryFilter _activeCategory = CatalogCategoryFilter.All;

        private int _activeTab;

        private List<SupportedBuffItemInfo> _catalog = new List<SupportedBuffItemInfo>();
        private List<SupportedBuffItemInfo> _filteredCatalog = new List<SupportedBuffItemInfo>();
        private ItemFilterMode _mode = ItemFilterMode.Disabled;

        private DraggablePanel _panel;
        private TextInput _searchInput;
        private string _searchText = string.Empty;
        private List<SupportedBuffItemInfo> _selectedItems = new List<SupportedBuffItemInfo>();

        public void Initialize(ModContext context)
        {
            var panel = new DraggablePanel(
                "auto-reapply-potions",
                "Auto Reapply Potions - Filter Editor",
                PanelWidth,
                PanelHeight);
            _panel = panel;

            _panel.ClipContent = false;
            _panel.OnClose = OnPanelClose;
            _panel.RegisterDrawCallback(DrawUi);

            var searchInput = new TextInput("Search supported buff items...", maxLength: 100);
            _searchInput = searchInput;

            RefreshFromConfig();
        }

        public void Unload()
        {
            _panel?.UnregisterDrawCallback();
            _panel = null;
            _searchInput = null;

            _catalog.Clear();
            _filteredCatalog.Clear();
            _selectedItems.Clear();
            _visibleCategoryTabs.Clear();
        }

        public void Toggle()
        {
            EnsureCatalogBuilt();
            RefreshFromConfig();
            _panel?.Toggle();
        }

        public void RefreshFromConfig()
        {
            _mode = ItemFilterSettings.Mode;
            RebuildLists(resetScroll: false);
        }

        private void EnsureCatalogBuilt()
        {
            _catalog = SupportedBuffItemsCatalog.GetSortedItems();
            RebuildLists(resetScroll: true);
        }

        private void OnPanelClose()
        {
            _searchInput?.Unfocus();
        }

        private void RebuildLists(bool resetScroll)
        {
            IEnumerable<SupportedBuffItemInfo> catalogQuery = _catalog;
            catalogQuery = ApplyCategoryFilter(catalogQuery, _activeCategory);
            catalogQuery = ApplySearchFilter(catalogQuery, _searchText);

            _filteredCatalog = catalogQuery.OrderBy(x => x.Category).ThenBy(x => x.ItemName).ThenBy(x => x.ItemId)
                .ToList();

            IEnumerable<SupportedBuffItemInfo> selectedQuery =
                _catalog.Where(x => ItemFilterSettings.Contains(x.ItemId));

            selectedQuery = ApplyCategoryFilter(selectedQuery, _activeCategory);
            selectedQuery = ApplySearchFilter(selectedQuery, _searchText);

            _selectedItems = selectedQuery.OrderBy(x => x.Category).ThenBy(x => x.ItemName).ThenBy(x => x.ItemId)
                .ToList();

            RebuildVisibleCategoryTabs();
            EnsureValidActiveCategory();

            if (resetScroll)
            {
                _catalogScroll.ResetScroll();
                _selectedScroll.ResetScroll();
            }
        }

        private void RebuildVisibleCategoryTabs()
        {
            _visibleCategoryTabs.Clear();
            _visibleCategoryTabs.Add(CatalogCategoryFilter.All);

            var source = GetCurrentCategoryTabSource().ToList();

            if (source.Any(x => x.Category == SupportedBuffItemCategory.Potion))
                _visibleCategoryTabs.Add(CatalogCategoryFilter.Potions);

            if (source.Any(x => x.Category == SupportedBuffItemCategory.Food))
                _visibleCategoryTabs.Add(CatalogCategoryFilter.Food);

            if (source.Any(x => x.Category == SupportedBuffItemCategory.Flask))
                _visibleCategoryTabs.Add(CatalogCategoryFilter.Flasks);

            if (source.Any(x => x.Category == SupportedBuffItemCategory.Other))
                _visibleCategoryTabs.Add(CatalogCategoryFilter.Other);
        }

        private IEnumerable<SupportedBuffItemInfo> GetCurrentCategoryTabSource()
        {
            if (_activeTab == 0)
                return ApplySearchFilter(_catalog, _searchText);

            IEnumerable<SupportedBuffItemInfo> selectedSource =
                _catalog.Where(x => ItemFilterSettings.Contains(x.ItemId));
            return ApplySearchFilter(selectedSource, _searchText);
        }

        private void EnsureValidActiveCategory()
        {
            if (!_visibleCategoryTabs.Contains(_activeCategory))
                _activeCategory = CatalogCategoryFilter.All;
        }

        private void SaveCurrentSettings()
        {
            try
            {
                ItemFilterSettings.Update(_mode, ItemFilterSettings.GetSortedIds());

                AutoReapplyPotionsConfig config = AutoReapplyPotionsMod.Config;
                if (config != null)
                {
                    config.ItemFilterMode = _mode.ToString();
                    config.FilteredItemIds = ItemFilterSettings.SerializeToConfigString();
                    config.Save();
                }

                ModLog.Info($"Filter UI saved. Mode={_mode}, Count={ItemFilterSettings.FilteredItemIds.Count}");
            }
            catch (Exception ex)
            {
                ModLog.Error($"Failed to save filter settings from UI: {ex}");
            }
        }

        private void DrawUi()
        {
            if (_panel == null)
                return;

            _searchInput?.Update();

            if (!_panel.BeginDraw())
                return;

            try
            {
                int contentX = _panel.ContentX;
                int contentY = _panel.ContentY;
                int contentWidth = _panel.ContentWidth;
                int contentHeight = _panel.ContentHeight;

                int currentY = contentY;

                currentY = DrawHeaderSection(contentX, currentY, contentWidth);
                currentY += SectionSpacing;

                currentY = DrawMainTabs(contentX, currentY, contentWidth);
                currentY += SectionSpacing;

                currentY = DrawCategoryTabs(contentX, currentY, contentWidth);
                currentY += SectionSpacing;

                DrawListSection(contentX, currentY, contentWidth, contentHeight - (currentY - contentY));
            }
            finally
            {
                _panel.EndDraw();
            }
        }

        private int DrawHeaderSection(int x, int y, int width)
        {
            var layout = new StackLayout(x, y, width, spacing: SectionSpacing);

            layout.SectionHeader("Filter Mode");

            int buttonsRowY = layout.Advance(ControlHeight);
            int thirdWidth = (width - 12) / 3;

            DrawFilterModeButton(
                layout.X,
                buttonsRowY,
                thirdWidth,
                ItemFilterMode.Disabled,
                "Disabled",
                "Filter is ignored. All supported items may be used.",
                () =>
                {
                    _mode = ItemFilterMode.Disabled;
                    SaveCurrentSettings();
                });

            DrawFilterModeButton(
                layout.X + thirdWidth + 6,
                buttonsRowY,
                thirdWidth,
                ItemFilterMode.Blacklist,
                "Blacklist",
                "Selected items are excluded. All other supported items may be used.",
                () =>
                {
                    _mode = ItemFilterMode.Blacklist;
                    SaveCurrentSettings();
                });

            int lastButtonX = layout.X + (thirdWidth + 6) * 2;
            int lastButtonWidth = width - (thirdWidth * 2) - 12;

            DrawFilterModeButton(
                lastButtonX,
                buttonsRowY,
                lastButtonWidth,
                ItemFilterMode.Whitelist,
                "Whitelist",
                "Only selected items may be used. All other supported items are ignored.",
                () =>
                {
                    _mode = ItemFilterMode.Whitelist;
                    SaveCurrentSettings();
                });

            layout.Label($"Selected IDs: {ItemFilterSettings.FilteredItemIds.Count}");

            string newSearch = _searchInput.Draw(layout.X, layout.CurrentY, layout.Width);
            layout.Advance(ControlHeight);

            if (!string.Equals(_searchText, newSearch, StringComparison.Ordinal))
            {
                _searchText = newSearch ?? string.Empty;
                RebuildLists(resetScroll: true);
            }

            int actionButtonsY = layout.Advance(ControlHeight);
            int halfWidth = (width - 6) / 2;

            if (_activeTab == 0)
            {
                if (Button.Draw(layout.X, actionButtonsY, halfWidth, ControlHeight, "Select All Visible"))
                {
                    var changed = false;

                    foreach (SupportedBuffItemInfo item in _filteredCatalog)
                    {
                        if (ItemFilterSettings.Add(item.ItemId))
                            changed = true;
                    }

                    if (changed)
                    {
                        SaveCurrentSettings();
                        RebuildLists(resetScroll: false);
                    }
                }
            }
            else
            {
                if (Button.Draw(layout.X, actionButtonsY, halfWidth, ControlHeight, "Clear Visible"))
                {
                    var changed = false;

                    foreach (SupportedBuffItemInfo item in _selectedItems)
                    {
                        if (ItemFilterSettings.Remove(item.ItemId))
                            changed = true;
                    }

                    if (changed)
                    {
                        SaveCurrentSettings();
                        RebuildLists(resetScroll: false);
                    }
                }
            }

            if (Button.Draw(layout.X + halfWidth + 6, actionButtonsY, halfWidth, ControlHeight, "Clear Selected"))
            {
                ItemFilterSettings.Clear();
                SaveCurrentSettings();
                RebuildLists(resetScroll: false);
            }

            return layout.CurrentY;
        }

        private void DrawFilterModeButton(
            int x,
            int y,
            int width,
            ItemFilterMode mode,
            string label,
            string tooltip,
            Action onClick)
        {
            string buttonText = _mode == mode ? $"{label} *" : label;

            if (Button.Draw(x, y, width, ControlHeight, buttonText))
                onClick?.Invoke();

            if (WidgetInput.IsMouseOver(x, y, width, ControlHeight))
                Tooltip.Set(tooltip);
        }

        private int DrawMainTabs(int x, int y, int width)
        {
            string[] tabs = ["Catalog", "Selected"];

            // ReSharper disable once RedundantArgumentDefaultValue
            int newTab = TabBar.Draw(x, y, width, tabs, _activeTab, height: ControlHeight);

            if (newTab != _activeTab)
            {
                _activeTab = newTab;
                RebuildLists(resetScroll: true);
            }

            return y + ControlHeight;
        }

        private int DrawCategoryTabs(int x, int y, int width)
        {
            string[] labels = _visibleCategoryTabs.Select(GetCategoryLabel).ToArray();

            int selectedIndex = _visibleCategoryTabs.IndexOf(_activeCategory);
            if (selectedIndex < 0)
                selectedIndex = 0;

            // ReSharper disable once RedundantArgumentDefaultValue
            int newIndex = TabBar.Draw(x, y, width, labels, selectedIndex, height: ControlHeight);

            CatalogCategoryFilter newCategory = _visibleCategoryTabs[newIndex];
            if (newCategory != _activeCategory)
            {
                _activeCategory = newCategory;
                RebuildLists(resetScroll: true);
            }

            return y + ControlHeight;
        }

        private static string GetCategoryLabel(CatalogCategoryFilter category)
        {
            switch (category)
            {
                case CatalogCategoryFilter.Potions:
                    return "Potions";

                case CatalogCategoryFilter.Food:
                    return "Food";

                case CatalogCategoryFilter.Flasks:
                    return "Flasks";

                case CatalogCategoryFilter.Other:
                    return "Other";

                default:
                    return "All";
            }
        }

        private void DrawListSection(int x, int y, int width, int height)
        {
            int listHeight = Math.Max(MinListHeight, height);

            if (_activeTab == 0)
                DrawCatalogList(x, y, width, listHeight);
            else
                DrawSelectedList(x, y, width, listHeight);
        }

        private void DrawCatalogList(int x, int y, int width, int height)
        {
            int totalHeight = _filteredCatalog.Count * RowHeight;
            _catalogScroll.Begin(x, y, width, height, totalHeight);

            try
            {
                int contentX = _catalogScroll.ContentX;
                int contentY = _catalogScroll.ContentY;
                int contentWidth = _catalogScroll.ContentWidth;

                for (var i = 0; i < _filteredCatalog.Count; i++)
                {
                    int itemOffsetY = i * RowHeight;

                    if (!_catalogScroll.IsVisible(itemOffsetY, RowHeight))
                        continue;

                    int rowY = contentY + itemOffsetY;
                    DrawItemRow(_filteredCatalog[i], contentX, rowY, contentWidth);
                }
            }
            finally
            {
                _catalogScroll.End();
            }
        }

        private void DrawSelectedList(int x, int y, int width, int height)
        {
            int totalHeight = _selectedItems.Count * RowHeight;
            _selectedScroll.Begin(x, y, width, height, totalHeight);

            try
            {
                int contentX = _selectedScroll.ContentX;
                int contentY = _selectedScroll.ContentY;
                int contentWidth = _selectedScroll.ContentWidth;

                for (var i = 0; i < _selectedItems.Count; i++)
                {
                    int itemOffsetY = i * RowHeight;

                    if (!_selectedScroll.IsVisible(itemOffsetY, RowHeight))
                        continue;

                    int rowY = contentY + itemOffsetY;
                    DrawItemRow(_selectedItems[i], contentX, rowY, contentWidth);
                }
            }
            finally
            {
                _selectedScroll.End();
            }
        }

        private void DrawItemRow(SupportedBuffItemInfo info, int x, int y, int width)
        {
            bool isSelected = ItemFilterSettings.Contains(info.ItemId);
            bool hover = WidgetInput.IsMouseOver(x, y, width, RowHeight);

            if (hover)
                UIRenderer.DrawRect(x, y, width, RowHeight, UIColors.ItemHoverBg);

            UIRenderer.DrawItem(info.ItemId, x + 4, y + 3, 22, 22);

            var checkboxSize = 16;
            var checkboxMarginRight = 8;
            int checkboxX = x + width - checkboxMarginRight - checkboxSize;
            int checkboxY = y + 6;

            var label = $"[{info.ItemId}] {info.ItemName} ({info.Category})";
            UIRenderer.DrawText(label, x + 32, y + 6, UIColors.Text);

            bool clicked = Checkbox.Draw(checkboxX, checkboxY, checkboxSize, isSelected);

            bool shouldTreatRowClickAsCheckboxClick = !clicked && hover && WidgetInput.MouseLeftClick;
            if (shouldTreatRowClickAsCheckboxClick)
            {
                clicked = true;
                WidgetInput.ConsumeClick();
            }

            if (clicked)
            {
                ItemFilterSettings.Toggle(info.ItemId);
                SaveCurrentSettings();
                RebuildLists(resetScroll: false);
            }
        }

        private static IEnumerable<SupportedBuffItemInfo> ApplySearchFilter(
            IEnumerable<SupportedBuffItemInfo> source,
            string searchText)
        {
            string search = (searchText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(search))
                return source;

            string lower = search.ToLowerInvariant();

            return source.Where(x =>
                (!string.IsNullOrWhiteSpace(x.ItemName) && x.ItemName.ToLowerInvariant().Contains(lower)) ||
                x.ItemId.ToString().Contains(lower) || x.Category.ToString().ToLowerInvariant().Contains(lower));
        }

        private static IEnumerable<SupportedBuffItemInfo> ApplyCategoryFilter(
            IEnumerable<SupportedBuffItemInfo> source,
            CatalogCategoryFilter category)
        {
            switch (category)
            {
                case CatalogCategoryFilter.Potions:
                    return source.Where(x => x.Category == SupportedBuffItemCategory.Potion);

                case CatalogCategoryFilter.Food:
                    return source.Where(x => x.Category == SupportedBuffItemCategory.Food);

                case CatalogCategoryFilter.Flasks:
                    return source.Where(x => x.Category == SupportedBuffItemCategory.Flask);

                case CatalogCategoryFilter.Other:
                    return source.Where(x => x.Category == SupportedBuffItemCategory.Other);

                default:
                    return source;
            }
        }
    }
}