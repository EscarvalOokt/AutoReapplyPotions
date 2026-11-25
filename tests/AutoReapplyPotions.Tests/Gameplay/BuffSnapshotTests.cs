using NUnit.Framework;

namespace AutoReapplyPotions.Tests.Gameplay
{
    [TestFixture]
    public sealed class BuffSnapshotTests
    {
        [Test]
        public void Capture_SkipsBuffsThatCannotExpireThisTick()
        {
            var snapshot = BuffSnapshot.Capture(new[] { 5, 3, 0 }, new[] { 1200, 0, 1 });

            Assert.That(snapshot.Count, Is.Zero);
        }

        [Test]
        public void FindExpired_RequiresAnActualCountdownToZero()
        {
            var snapshot = BuffSnapshot.Capture(new[] { 5, 3, 7, 26 }, new[] { 600, 1, 1, 1 });

            // The first two buffs were evicted, buff 7 expired, and food was refreshed.
            BuffSnapshot expired = snapshot.FindExpired(new[] { 7, 26, 151 }, new[] { 0, 1800, 1 });

            Assert.That(expired.Count, Is.EqualTo(1));
            Assert.That(expired[0], Is.EqualTo(7));
        }

        [Test]
        public void FindExpired_PreservesSimultaneousExpirationsAcrossSlotChanges()
        {
            var snapshot = BuffSnapshot.Capture(new[] { 26, 5, 73 }, new[] { 1, 120, 1 });

            BuffSnapshot expired = snapshot.FindExpired(new[] { 73, 26, 5 }, new[] { 0, 0, 119 });

            Assert.That(expired.Count, Is.EqualTo(2));
            Assert.That(expired[0], Is.EqualTo(26));
            Assert.That(expired[1], Is.EqualTo(73));
        }

        [Test]
        public void FindExpired_RemembersAZeroTimerCompactedDuringUpdate()
        {
            var snapshot = BuffSnapshot.Capture(new[] { 5, 64, 3 }, new[] { 1, 18000, 60 });
            BuffSnapshot beforeRemoval = snapshot.FindExpired(new[] { 5, 64, 3 }, new[] { 0, 18000, 60 });

            BuffSnapshot expired = snapshot.FindExpired(new[] { 3, 0, 0 }, new[] { 59, 0, 0 }, beforeRemoval);
            BuffSnapshot refreshed = snapshot.FindExpired(new[] { 3, 5, 0 }, new[] { 59, 300, 0 }, beforeRemoval);

            Assert.That(expired.Count, Is.EqualTo(1));
            Assert.That(expired[0], Is.EqualTo(5));
            Assert.That(refreshed.Count, Is.Zero);
        }

        [Test]
        public void FindExpired_ProcessesEachBuffOnceAndSkipsStillActiveDuplicates()
        {
            var snapshot = BuffSnapshot.Capture(new[] { 3, 5, 5, 3 }, new[] { 1, 1, 1, 40 });

            BuffSnapshot expired = snapshot.FindExpired(new[] { 3, 5, 5, 3 }, new[] { 0, 0, 0, 39 });

            Assert.That(expired.Count, Is.EqualTo(1));
            Assert.That(expired[0], Is.EqualTo(5));
        }
    }
}