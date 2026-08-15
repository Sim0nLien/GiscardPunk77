using GiscardPunk77.Gameplay.Doors;
using NUnit.Framework;

namespace GiscardPunk77.Gameplay.Tests
{
    public sealed class DoorReservationQueueTests
    {
        [Test]
        public void TwoOwnersAreGrantedInFirstInFirstOutOrder()
        {
            var queue = new DoorReservationQueue();
            var first = new object();
            var second = new object();

            Assert.That(queue.TryReserve(first, 0f, 5f), Is.True);
            Assert.That(queue.TryReserve(second, 0.1f, 5f), Is.False);
            Assert.That(queue.ActiveOwner, Is.SameAs(first));

            Assert.That(queue.Release(first), Is.True);
            Assert.That(queue.TryReserve(second, 0.2f, 5f), Is.True);
            Assert.That(queue.ActiveOwner, Is.SameAs(second));
        }

        [Test]
        public void FourOwnersRemainFairAcrossTwentyAlternatingPassages()
        {
            var queue = new DoorReservationQueue();
            var owners = new[] { new object(), new object(), new object(), new object() };
            for (var index = 0; index < owners.Length; index++)
            {
                queue.TryReserve(owners[index], 0f, 100f);
            }

            for (var passage = 0; passage < 20; passage++)
            {
                var expectedOwner = owners[passage % owners.Length];
                Assert.That(queue.ActiveOwner, Is.SameAs(expectedOwner));
                Assert.That(queue.Release(expectedOwner), Is.True);
                queue.TryReserve(expectedOwner, passage + 0.1f, 100f);
            }

            Assert.That(queue.Count, Is.EqualTo(4));
        }

        [Test]
        public void AbandonedReservationExpiresAndPromotesNextOwner()
        {
            var queue = new DoorReservationQueue();
            var abandoned = new object();
            var waiting = new object();

            queue.TryReserve(abandoned, 0f, 1f);
            queue.TryReserve(waiting, 0.5f, 2f);

            Assert.That(queue.RemoveExpired(1.1f), Is.EqualTo(1));
            Assert.That(queue.ActiveOwner, Is.SameAs(waiting));
            Assert.That(queue.TryReserve(waiting, 1.1f, 2f), Is.True);
        }

        [Test]
        public void ReleaseAndClearAreIdempotent()
        {
            var queue = new DoorReservationQueue();
            var owner = new object();
            queue.TryReserve(owner, 0f, 5f);

            Assert.That(queue.Release(owner), Is.True);
            Assert.That(queue.Release(owner), Is.False);
            queue.Clear();
            queue.Clear();

            Assert.That(queue.Count, Is.Zero);
            Assert.That(queue.ActiveOwner, Is.Null);
        }
    }
}
