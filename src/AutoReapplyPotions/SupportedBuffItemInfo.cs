namespace AutoReapplyPotions
{
    internal sealed class SupportedBuffItemInfo(int itemId, string itemName, SupportedBuffItemCategory category)
    {
        public int ItemId { get; } = itemId;
        public string ItemName { get; } = itemName;

        public SupportedBuffItemCategory Category { get; } = category;
    }
}