using NUnit.Framework;

namespace AutoReapplyPotions.Tests.UI
{
    [TestFixture]
    public sealed class FilterEditorClickAwayTests
    {
        [TestCase(true, false, true, false, false, true, TestName = "AvailableClickSignal_TriggersClickAway")]
        [TestCase(
            true,
            false,
            false,
            true,
            false,
            true,
            TestName = "HigherPanelMasksClickAndHover_PhysicalPressReleasesSearch")]
        [TestCase(true, false, false, true, true, false, TestName = "HeldMouseButton_DoesNotRepeatClickAway")]
        [TestCase(false, false, true, true, false, false, TestName = "UnfocusedSearch_DoesNotTriggerClickAway")]
        [TestCase(true, true, true, true, false, false, TestName = "PressInsideSearch_KeepsFocus")]
        [TestCase(
            true,
            false,
            false,
            false,
            true,
            false,
            TestName = "ReleasedMouseWithoutClick_DoesNotTriggerClickAway")]
        public void ShouldReleaseFocus_RespectsFocusAndMouseOwnership(
            bool hasTextFocus,
            bool pointerOverSearch,
            bool mouseLeftClick,
            bool mouseLeftDown,
            bool wasMouseLeftDown,
            bool expected)
        {
            bool actual = FilterEditorClickAwayPolicy.ShouldReleaseFocus(
                hasTextFocus,
                pointerOverSearch,
                mouseLeftClick,
                mouseLeftDown,
                wasMouseLeftDown);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}