using System;
using System.Collections.Generic;
using Escarval.TerrariaWindowKit;
using Terraria.ID;
using TerrariaModder.Core;
using TerrariaModder.Core.UI;
using TerrariaModder.Core.UI.Widgets;

namespace AutoReapplyPotions
{
    internal sealed class FilterUiController
    {
        private const int PanelWidth = 520;
        private const int PanelHeight = 420;
        private const int HeaderHeight = 32;
        private const int PanelPadding = 8;
        private const int CloseButtonSize = 24;
        private const int PinButtonSpacing = 6;
        private const string PanelTitle = "Auto Reapply Potions - Filter Editor";

        private const int RowHeight = 26;
        private const int SectionSpacing = 4;
        private const int ControlHeight = 26;
        private const int CategoryButtonHeight = 28;
        private const int CategoryIconButtonWidth = 32;
        private const int CategoryButtonSpacing = 4;

        private static readonly SupportedBuffItemCategory?[] _categoryOrder =
        [
            null, // All categories
            SupportedBuffItemCategory.Potion,
            SupportedBuffItemCategory.Food,
            SupportedBuffItemCategory.Flask,
            SupportedBuffItemCategory.Other
        ];

        private readonly ScrollView _catalogScroll = new ScrollView();

        private readonly FilterEditorResults _results = new FilterEditorResults();
        private readonly ScrollView _selectedScroll = new ScrollView();
        private SupportedBuffItemCategory? _activeCategory;

        private int _activeTab;

        private CoreWindowBackend _backend;
        private List<SupportedBuffItemInfo> _catalog = new List<SupportedBuffItemInfo>();
        private int _listTopOffset = -1;
        private ItemFilterMode _mode = ItemFilterMode.Disabled;
        private TextInput _searchInput;
        private string _searchText = string.Empty;
        private bool _wasMouseLeftDown;
        private WindowHost _window;

        public void Initialize(ModContext context)
        {
            _searchInput = new TextInput("Search supported buff items...", maxLength: 100);

            RefreshFromConfig();
        }

        public void Unload()
        {
            try
            {
                _window?.Dispose();
            }
            finally
            {
                _window = null;
                try
                {
                    _backend?.Dispose();
                }
                finally
                {
                    _backend = null;
                    _searchInput = null;
                    _wasMouseLeftDown = false;
                    _listTopOffset = -1;

                    _catalog.Clear();
                    _results.Clear();
                }
            }
        }

        public void Toggle()
        {
            EnsureCatalogBuilt();
            RefreshFromConfig();
            EnsureWindowCreated();
            _window.Toggle();
            _wasMouseLeftDown = UIRenderer.MouseLeft;
        }

        private void EnsureWindowCreated()
        {
            if (_window != null)
                return;

            // Create the backend on the UI thread when the editor is first opened.
            var backend = new CoreWindowBackend(SharedCoordinator.Connect());
            try
            {
                // Keep full mode labels readable even when the selected-item count reaches three digits.
                int modeLabelWidth = Math.Max(
                    UIRenderer.MeasureText("Disabled (999 saved)"),
                    Math.Max(UIRenderer.MeasureText("Blacklist (999)"), UIRenderer.MeasureText("Whitelist (999)")));
                int windowWidth = Math.Max(PanelWidth, (modeLabelWidth + 16) * 3 + 12 + PanelPadding * 2);

                var options = new WindowOptions(
                    new WindowBounds(
                        Math.Max(0, (backend.ViewportWidth - windowWidth) / 2),
                        Math.Max(0, (backend.ViewportHeight - PanelHeight) / 2),
                        windowWidth,
                        PanelHeight))
                {
                    Draggable = true,
                    Resizable = false,
                    Pinnable = true,
                    MinimumWidth = windowWidth,
                    MinimumHeight = PanelHeight
                };
                var callbacks = new WindowCallbacks
                {
                    Draw = DrawUi,
                    HasTextFocus = () => _searchInput?.IsFocused == true,
                    ReleaseTextFocus = OnPanelClose,
                    WheelAt = GetWheelDisposition
                };

                _window = new WindowHost("auto-reapply-potions", "filter", options, backend, callbacks);
                _backend = backend;
            }
            catch
            {
                backend.Dispose();
                throw;
            }
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
            _results.Rebuild(_catalog, _activeCategory, _searchText, _activeTab != 0, ItemFilterSettings.Contains);

            if (resetScroll)
            {
                _catalogScroll.ResetScroll();
                _selectedScroll.ResetScroll();
            }
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

                ModLog.Info($"Filter applied. Mode={_mode}, Count={ItemFilterSettings.FilteredItemIds.Count}");
            }
            catch (Exception ex)
            {
                ModLog.Error($"Failed to save filter settings from UI: {ex}");
            }
        }

        private void DrawUi()
        {
            WindowHost window = _window;
            if (window == null || !window.IsOpen)
                return;

            int mouseX = WidgetInput.MouseX;
            int mouseY = WidgetInput.MouseY;
            bool mouseLeftDown = UIRenderer.MouseLeft;
            // Header and mode buttons may consume this click before the search field draws.
            bool mouseLeftClick = WidgetInput.MouseLeftClick;
            window.UpdatePointer(mouseX, mouseY, mouseLeftDown, WidgetInput.BlockInput);
            WindowBounds bounds = window.Bounds;

            int closeX = bounds.X + bounds.Width - PanelPadding - CloseButtonSize;
            int closeY = bounds.Y + (HeaderHeight - CloseButtonSize) / 2;
            bool hasCloseButton = bounds is { Width: >= CloseButtonSize + PanelPadding * 2, Height: >= HeaderHeight };
            bool closeHovered = hasCloseButton &&
                                WidgetInput.IsMouseOver(closeX, closeY, CloseButtonSize, CloseButtonSize);

            int pinWidth = CorePinButtonRenderer.MeasureWidth("Pin", "Unpin");
            int pinX = closeX - PinButtonSpacing - pinWidth;
            bool hasPinButton = hasCloseButton &&
                                bounds.Width >= (long)PanelPadding * 2 + CloseButtonSize + PinButtonSpacing + pinWidth;
            bool pinHovered = hasPinButton &&
                              !WidgetInput.BlockInput &&
                              WidgetInput.IsMouseOver(pinX, closeY, pinWidth, CloseButtonSize);

            if (!WidgetInput.BlockInput && WidgetInput.MouseLeftClick)
            {
                if (pinHovered)
                {
                    window.SetPinned(!window.IsPinned);
                    WidgetInput.ConsumeClick();
                }
                else if (!closeHovered &&
                         WidgetInput.IsMouseOver(
                             bounds.X,
                             bounds.Y,
                             bounds.Width,
                             Math.Min(HeaderHeight, bounds.Height)))
                {
                    window.BeginDrag(mouseX, mouseY);
                    WidgetInput.ConsumeClick();
                }
            }

            UIRenderer.DrawPanel(bounds.X, bounds.Y, bounds.Width, bounds.Height, UIColors.PanelBg);
            UIRenderer.DrawRect(
                bounds.X + 1,
                bounds.Y + 1,
                Math.Max(0, bounds.Width - 2),
                Math.Max(0, Math.Min(HeaderHeight, bounds.Height - 2)),
                UIColors.HeaderBg);

            int titleWidth = bounds.Width - PanelPadding * 3 - CloseButtonSize;
            if (hasPinButton)
                titleWidth -= pinWidth + PinButtonSpacing;

            if (titleWidth > 0 && bounds.Height >= HeaderHeight)
            {
                UIRenderer.DrawText(
                    TextUtil.Truncate(PanelTitle, titleWidth),
                    bounds.X + PanelPadding + 4,
                    bounds.Y + 7,
                    UIColors.TextTitle);
            }

            if (hasPinButton)
            {
                CorePinButtonRenderer.Draw(
                    pinX,
                    closeY,
                    pinWidth,
                    CloseButtonSize,
                    window.IsPinned,
                    pinHovered,
                    "Pin",
                    "Unpin");

                if (pinHovered)
                    Tooltip.Set(window.IsPinned ? "Unpin window" : "Pin window");
            }

            if (hasCloseButton &&
                Button.Draw(
                    closeX,
                    closeY,
                    CloseButtonSize,
                    CloseButtonSize,
                    "X",
                    UIColors.CloseBtn,
                    UIColors.CloseBtnHover,
                    UIColors.Text))
            {
                window.Close();
                return;
            }

            try
            {
                int contentX = bounds.X + PanelPadding;
                int contentY = bounds.Y + HeaderHeight + PanelPadding;
                int contentWidth = Math.Max(0, bounds.Width - PanelPadding * 2);
                int contentHeight = Math.Max(0, bounds.Height - HeaderHeight - PanelPadding * 2);

                int currentY = contentY;

                currentY = DrawFilterModeAndSearch(contentX, currentY, contentWidth, mouseLeftClick);
                currentY += SectionSpacing;

                currentY = DrawMainTabs(contentX, currentY, contentWidth);
                currentY += SectionSpacing;

                currentY = DrawCategoryTabs(contentX, currentY, contentWidth);
                currentY += SectionSpacing;

                currentY = DrawBulkActions(contentX, currentY, contentWidth);
                currentY += SectionSpacing;

                _listTopOffset = currentY - bounds.Y;
                DrawListSection(contentX, currentY, contentWidth, contentHeight - (currentY - contentY));
            }
            finally
            {
                Tooltip.DrawDeferred();
            }
        }

        private int GetWheelDisposition(int x, int y)
        {
            WindowHost window = _window;
            if (window == null || !window.IsOpen || _listTopOffset < 0)
                return (int)WheelDisposition.PassThrough;

            WindowBounds bounds = window.Bounds;
            int listX = bounds.X + PanelPadding;
            int listY = bounds.Y + _listTopOffset;
            int listWidth = bounds.Width - PanelPadding * 2;
            int listHeight = bounds.Height - _listTopOffset - PanelPadding;

            return listWidth > 0 &&
                   listHeight > 0 &&
                   x >= listX &&
                   x < (long)listX + listWidth &&
                   y >= listY &&
                   y < (long)listY + listHeight
                ? (int)WheelDisposition.Deliver
                : (int)WheelDisposition.PassThrough;
        }

        private int DrawFilterModeAndSearch(int x, int y, int width, bool mouseLeftClick)
        {
            var layout = new StackLayout(x, y, width, spacing: SectionSpacing);

            int buttonsRowY = layout.Advance(ControlHeight);
            int thirdWidth = Math.Max(0, (width - 12) / 3);

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
            int lastButtonWidth = Math.Max(0, width - thirdWidth * 2 - 12);

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

            int searchY = layout.CurrentY;
            bool mouseLeftDown = UIRenderer.MouseLeft;
            if (FilterEditorClickAwayPolicy.ShouldReleaseFocus(
                    _searchInput.IsFocused,
                    WidgetInput.IsMouseOver(layout.X, searchY, width, ControlHeight),
                    mouseLeftClick,
                    mouseLeftDown,
                    _wasMouseLeftDown))
                _searchInput.Unfocus();

            _wasMouseLeftDown = mouseLeftDown;
            _searchInput.Update();
            string newSearch = _searchInput.Draw(layout.X, searchY, width, height: ControlHeight);

            if (width > 0)
            {
                UIRenderer.DrawRectOutline(
                    layout.X,
                    searchY,
                    width,
                    ControlHeight,
                    _searchInput.IsFocused ? UIColors.Accent : UIColors.Border);
            }

            layout.Advance(ControlHeight);

            if (!string.Equals(_searchText, newSearch, StringComparison.Ordinal))
            {
                _searchText = newSearch ?? string.Empty;
                RebuildLists(resetScroll: true);
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
            if (width <= 0)
                return;

            bool active = _mode == mode;
            int count = ItemFilterSettings.FilteredItemIds.Count;
            string buttonText = !active ? label :
                mode == ItemFilterMode.Disabled ? $"{label} ({count} saved)" : $"{label} ({count})";

            if (Button.Draw(
                    x,
                    y,
                    width,
                    ControlHeight,
                    buttonText,
                    active ? UIColors.ItemActiveBg : UIColors.Button,
                    active ? UIColors.ItemActiveBg : UIColors.ButtonHover,
                    UIColors.Text))
                onClick?.Invoke();

            if (active)
                UIRenderer.DrawRectOutline(x, y, width, ControlHeight, UIColors.Accent);

            if (WidgetInput.IsMouseOver(x, y, width, ControlHeight))
            {
                if (mode == ItemFilterMode.Whitelist && active && count == 0)
                    Tooltip.Set("Whitelist is empty. No items are allowed for automatic buff reapplication.");
                else
                    Tooltip.Set(tooltip);
            }
        }

        private int DrawBulkActions(int x, int y, int width)
        {
            int clearWidth = Math.Min(120, Math.Max(0, width / 3));
            int actionWidth = Math.Min(190, Math.Max(0, width - clearWidth - SectionSpacing));
            int actionX = x + Math.Max(0, width - actionWidth - clearWidth - SectionSpacing);
            int clearX = actionX + actionWidth + SectionSpacing;

            if (actionWidth > 0 &&
                Button.Draw(
                    actionX,
                    y,
                    actionWidth,
                    ControlHeight,
                    _activeTab == 0 ? "Select results" : "Clear results"))
            {
                var changed = false;

                if (_activeTab == 0)
                {
                    foreach (SupportedBuffItemInfo item in _results.Catalog)
                    {
                        if (ItemFilterSettings.Add(item.ItemId))
                            changed = true;
                    }
                }
                else
                {
                    foreach (SupportedBuffItemInfo item in _results.Selected)
                    {
                        if (ItemFilterSettings.Remove(item.ItemId))
                            changed = true;
                    }
                }

                if (changed)
                {
                    SaveCurrentSettings();
                    RebuildLists(resetScroll: false);
                }
            }

            if (clearWidth > 0 &&
                Button.Draw(
                    clearX,
                    y,
                    clearWidth,
                    ControlHeight,
                    "Clear all",
                    UIColors.ItemBg,
                    UIColors.ButtonHover,
                    UIColors.TextDim))
            {
                ItemFilterSettings.Clear();
                SaveCurrentSettings();
                RebuildLists(resetScroll: false);
            }

            return y + ControlHeight;
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
            if (width <= 0)
                return y;

            int groupWidth = CategoryButtonSpacing * (_categoryOrder.Length - 1);
            foreach (SupportedBuffItemCategory? category in _categoryOrder)
                groupWidth += GetCategoryButtonWidth(category);

            int startX = x + Math.Max(0, (width - groupWidth) / 2);
            int currentX = startX;
            int rowY = y;

            foreach (SupportedBuffItemCategory? category in _categoryOrder)
            {
                int desiredWidth = GetCategoryButtonWidth(category);
                int buttonWidth = Math.Min(width, desiredWidth);

                if (currentX > startX && currentX + buttonWidth > x + width)
                {
                    startX = x;
                    currentX = x;
                    rowY += CategoryButtonHeight + CategoryButtonSpacing;
                }

                bool enabled = !category.HasValue || _results.AvailableCategories.Contains(category.Value);
                bool active = _activeCategory == category;
                bool hovered = !WidgetInput.BlockInput &&
                               WidgetInput.IsMouseOver(currentX, rowY, buttonWidth, CategoryButtonHeight);

                UIRenderer.DrawRect(
                    currentX,
                    rowY,
                    buttonWidth,
                    CategoryButtonHeight,
                    active ? UIColors.ItemActiveBg : enabled && hovered ? UIColors.ButtonHover : UIColors.SectionBg);
                UIRenderer.DrawRectOutline(
                    currentX,
                    rowY,
                    buttonWidth,
                    CategoryButtonHeight,
                    active ? UIColors.Accent : UIColors.Border);

                int itemId = GetCategoryIconItemId(category);
                if (itemId != 0 && buttonWidth >= 22)
                {
                    int iconX = currentX + (buttonWidth - 22) / 2;
                    UIRenderer.DrawItem(itemId, iconX, rowY + 3, 22, 22);
                    if (!enabled)
                        UIRenderer.DrawRect(iconX, rowY + 3, 22, 22, UIColors.SectionBg.WithAlpha(160));
                }
                else if (itemId == 0)
                {
                    string text = TextUtil.Truncate(
                        FilterEditorResults.GetCategoryLabel(category),
                        Math.Max(0, buttonWidth - 4));
                    int textWidth = UIRenderer.MeasureText(text);
                    if (textWidth <= buttonWidth - 4)
                    {
                        UIRenderer.DrawText(
                            text,
                            currentX + (buttonWidth - textWidth) / 2,
                            rowY + 6,
                            enabled ? UIColors.Text : UIColors.TextDim);
                    }
                }

                if (hovered)
                    Tooltip.Set(FilterEditorResults.GetCategoryLabel(category) + (enabled ? "" : " (no results)"));

                if (enabled && hovered && WidgetInput.MouseLeftClick && category != _activeCategory)
                {
                    WidgetInput.ConsumeClick();
                    _activeCategory = category;
                    RebuildLists(resetScroll: true);
                }

                currentX += buttonWidth + CategoryButtonSpacing;
            }

            return rowY + CategoryButtonHeight;
        }

        private static int GetCategoryButtonWidth(SupportedBuffItemCategory? category)
        {
            return category switch
            {
                null => 48,
                SupportedBuffItemCategory.Other => 66,
                _ => CategoryIconButtonWidth
            };
        }

        private static int GetCategoryIconItemId(SupportedBuffItemCategory? category)
        {
            return category switch
            {
                SupportedBuffItemCategory.Potion => ItemID.IronskinPotion,
                SupportedBuffItemCategory.Food => ItemID.CookedFish,
                SupportedBuffItemCategory.Flask => ItemID.FlaskofFire,
                _ => 0
            };
        }

        private void DrawListSection(int x, int y, int width, int height)
        {
            int listHeight = Math.Max(0, height);
            if (width <= 0 || listHeight <= 0)
                return;

            if (_activeTab == 0)
                DrawCatalogList(x, y, width, listHeight);
            else
                DrawSelectedList(x, y, width, listHeight);
        }

        private void DrawCatalogList(int x, int y, int width, int height)
        {
            int totalHeight = _results.Catalog.Count * RowHeight;
            _catalogScroll.Begin(x, y, width, height, totalHeight);

            try
            {
                int contentX = _catalogScroll.ContentX;
                int contentY = _catalogScroll.ContentY;
                int contentWidth = _catalogScroll.ContentWidth;

                if (_results.Catalog.Count == 0)
                    DrawEmptyListMessage("No matching items", contentX, contentY, contentWidth);

                for (var i = 0; i < _results.Catalog.Count; i++)
                {
                    int itemOffsetY = i * RowHeight;

                    if (!_catalogScroll.IsVisible(itemOffsetY, RowHeight))
                        continue;

                    int rowY = contentY + itemOffsetY;
                    DrawItemRow(_results.Catalog[i], contentX, rowY, contentWidth);
                }
            }
            finally
            {
                _catalogScroll.End();
            }
        }

        private void DrawSelectedList(int x, int y, int width, int height)
        {
            int totalHeight = _results.Selected.Count * RowHeight;
            _selectedScroll.Begin(x, y, width, height, totalHeight);

            try
            {
                int contentX = _selectedScroll.ContentX;
                int contentY = _selectedScroll.ContentY;
                int contentWidth = _selectedScroll.ContentWidth;

                if (_results.Selected.Count == 0)
                {
                    string emptyMessage = ItemFilterSettings.FilteredItemIds.Count == 0
                        ? "No items selected"
                        : "No matching selected items";
                    DrawEmptyListMessage(emptyMessage, contentX, contentY, contentWidth);
                }

                for (var i = 0; i < _results.Selected.Count; i++)
                {
                    int itemOffsetY = i * RowHeight;

                    if (!_selectedScroll.IsVisible(itemOffsetY, RowHeight))
                        continue;

                    int rowY = contentY + itemOffsetY;
                    DrawItemRow(_results.Selected[i], contentX, rowY, contentWidth);
                }
            }
            finally
            {
                _selectedScroll.End();
            }
        }

        private static void DrawEmptyListMessage(string message, int x, int y, int width)
        {
            string text = TextUtil.Truncate(message, Math.Max(0, width - 16));
            if (UIRenderer.MeasureText(text) <= width - 16)
                UIRenderer.DrawText(text, x + 8, y + 8, UIColors.TextDim);
        }

        private void DrawItemRow(SupportedBuffItemInfo info, int x, int y, int width)
        {
            bool isSelected = ItemFilterSettings.Contains(info.ItemId);
            bool hover = WidgetInput.IsMouseOver(x, y, width, RowHeight);

            if (hover)
            {
                UIRenderer.DrawRect(x, y, width, RowHeight, UIColors.ItemHoverBg);
                Tooltip.Set(info.ItemName, $"Item ID: {info.ItemId} | Category: {info.Category}");
            }

            const int checkboxSize = 16;
            const int iconSize = 22;
            const int horizontalGap = 6;

            int checkboxX = x + 4;
            int checkboxY = y + (RowHeight - checkboxSize) / 2;
            int iconX = checkboxX + checkboxSize + horizontalGap;
            int iconY = y + (RowHeight - iconSize) / 2;
            int labelX = iconX + iconSize + horizontalGap;
            int labelWidth = Math.Max(0, x + width - labelX - 4);

            UIRenderer.DrawItem(info.ItemId, iconX, iconY, iconSize, iconSize);

            if (labelWidth > 0)
            {
                string label = TextUtil.Truncate(info.ItemName, labelWidth);
                if (UIRenderer.MeasureText(label) <= labelWidth)
                    UIRenderer.DrawText(label, labelX, y + 5, UIColors.Text);
            }

            bool clicked = Checkbox.Draw(checkboxX, checkboxY, checkboxSize, isSelected);
            bool rowClick = !clicked && hover && WidgetInput.MouseLeftClick;

            if (rowClick)
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
    }

    // Keep this input decision independent of Terraria/Core UI so it can be tested without the game runtime.
    internal static class FilterEditorClickAwayPolicy
    {
        internal static bool ShouldReleaseFocus(
            bool hasTextFocus,
            bool pointerOverSearch,
            bool mouseLeftClick,
            bool mouseLeftDown,
            bool wasMouseLeftDown)
        {
            // A higher panel masks the Core click and hover, but the physical press still transfers focus.
            bool pointerPressed = mouseLeftClick || (mouseLeftDown && !wasMouseLeftDown);
            return hasTextFocus && pointerPressed && !pointerOverSearch;
        }
    }
}