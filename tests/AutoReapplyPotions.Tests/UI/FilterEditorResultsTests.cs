using System.Linq;
using NUnit.Framework;

namespace AutoReapplyPotions.Tests.UI
{
    [TestFixture]
    public sealed class FilterEditorResultsTests
    {
        private readonly SupportedBuffItemInfo[] _catalog =
        {
            new SupportedBuffItemInfo(292, "Ironskin", SupportedBuffItemCategory.Potion),
            new SupportedBuffItemInfo(2425, "Cooked Fish", SupportedBuffItemCategory.Food),
            new SupportedBuffItemInfo(1354, "Fire", SupportedBuffItemCategory.Flask),
            new SupportedBuffItemInfo(9001, "Test item", SupportedBuffItemCategory.Other)
        };

        [TestCase(false)]
        [TestCase(true)]
        public void SearchOutsideChosenCategory_LeavesBulkResultsEmpty(bool selectedTab)
        {
            var results = new FilterEditorResults();
            results.Rebuild(_catalog, SupportedBuffItemCategory.Food, "Ironskin", selectedTab, id => true);

            Assert.That(results.Catalog, Is.Empty);
            Assert.That(results.Selected, Is.Empty);
            Assert.That(results.AvailableCategories, Is.EquivalentTo(new[] { SupportedBuffItemCategory.Potion }));

            results.Rebuild(_catalog, SupportedBuffItemCategory.Food, "", selectedTab, id => true);
            Assert.That(results.Catalog.Select(item => item.ItemId), Is.EqualTo(new[] { 2425 }));
            Assert.That(results.Selected.Select(item => item.ItemId), Is.EqualTo(new[] { 2425 }));
        }

        [TestCase("Potion", 292)]
        [TestCase(" POTIONS ", 292)]
        [TestCase("Flask", 1354)]
        [TestCase("flasks", 1354)]
        [TestCase("Other", 9001)]
        [TestCase("cooked", 2425)]
        [TestCase("425", 2425)]
        public void CatalogSearch_MatchesCategoryNameItemNameOrId(string search, int expectedId)
        {
            var results = new FilterEditorResults();
            results.Rebuild(_catalog, null, search, false, id => false);

            Assert.That(results.Catalog.Select(item => item.ItemId), Is.EqualTo(new[] { expectedId }));
            Assert.That(results.Selected, Is.Empty);
        }

        [Test]
        public void SelectedTab_OffersOnlyCategoriesWithMatchingSelectedItems()
        {
            var results = new FilterEditorResults();
            results.Rebuild(_catalog, null, "", true, id => id == 1354);

            Assert.That(results.AvailableCategories, Is.EquivalentTo(new[] { SupportedBuffItemCategory.Flask }));
            Assert.That(results.Selected.Select(item => item.ItemId), Is.EqualTo(new[] { 1354 }));

            results.Rebuild(_catalog, SupportedBuffItemCategory.Flask, "", true, id => false);
            Assert.That(results.AvailableCategories, Is.Empty);
            Assert.That(results.Selected, Is.Empty);
        }
    }
}