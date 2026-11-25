using NUnit.Framework;

namespace AutoReapplyPotions.Tests.Config
{
    [TestFixture]
    internal sealed class ItemFilterModeTests
    {
        [TestCase("Disabled", ItemFilterMode.Disabled)]
        [TestCase("Blacklist", ItemFilterMode.Blacklist)]
        [TestCase("whiTELIst", ItemFilterMode.Whitelist)]
        [TestCase("1", ItemFilterMode.Blacklist)]
        [TestCase("99", ItemFilterMode.Disabled)]
        [TestCase("-1", ItemFilterMode.Disabled)]
        [TestCase("invalid", ItemFilterMode.Disabled)]
        [TestCase("", ItemFilterMode.Disabled)]
        [TestCase(null, ItemFilterMode.Disabled)]
        public void ParseMode_AcceptsOnlyDefinedValues(string raw, ItemFilterMode expected)
        {
            Assert.That(ItemFilterSettings.ParseMode(raw), Is.EqualTo(expected));
        }
    }
}