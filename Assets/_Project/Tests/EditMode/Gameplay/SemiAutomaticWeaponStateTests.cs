using GiscardPunk77.Gameplay.Weapons;
using NUnit.Framework;

namespace GiscardPunk77.Gameplay.Tests
{
    public sealed class SemiAutomaticWeaponStateTests
    {
        [Test]
        public void DefaultWeaponContractIsEightShotsAndOnePointSixSecondReload()
        {
            Assert.That(PlayerHitscanWeapon.DefaultMagazineCapacity, Is.EqualTo(8));
            Assert.That(PlayerHitscanWeapon.DefaultReloadDuration, Is.EqualTo(1.6f));
        }

        [Test]
        public void EightShotsEmptyMagazineAndNinthShotDoesNotMakeAmmoNegative()
        {
            var state = CreateState(8, 24, 0.25f, 1.6f);

            for (var shot = 0; shot < 8; shot++)
            {
                Assert.That(state.TryConsumeShot(shot * 0.25f), Is.True);
            }

            Assert.That(state.TryConsumeShot(2f), Is.False);
            Assert.That(state.MagazineAmmo, Is.EqualTo(0));
            Assert.That(state.ReserveAmmo, Is.EqualTo(24));
        }

        [Test]
        public void CadenceRejectsShotBeforeIntervalHasElapsed()
        {
            var state = CreateState(8, 24, 0.25f, 1.6f);

            Assert.That(state.TryConsumeShot(10f), Is.True);
            Assert.That(state.TryConsumeShot(10.24f), Is.False);
            Assert.That(state.TryConsumeShot(10.25f), Is.True);
            Assert.That(state.MagazineAmmo, Is.EqualTo(6));
        }

        [Test]
        public void ReloadBlocksFireAndCompletesAtOnePointSixSeconds()
        {
            var state = CreateState(8, 24, 0.25f, 1.6f);
            state.TryConsumeShot(0f);

            Assert.That(state.TryStartReload(0.25f), Is.True);
            Assert.That(state.TryConsumeShot(1f), Is.False);
            Assert.That(state.CompleteReloadIfReady(1.849f), Is.False);
            Assert.That(state.CompleteReloadIfReady(1.851f), Is.True);
            Assert.That(state.MagazineAmmo, Is.EqualTo(8));
            Assert.That(state.ReserveAmmo, Is.EqualTo(23));
            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void ReloadTransfersOnlyAvailableReserve()
        {
            var state = CreateState(8, 2, 0.25f, 1.6f);
            state.TryConsumeShot(0f);
            state.TryConsumeShot(0.25f);
            state.TryConsumeShot(0.5f);

            Assert.That(state.TryStartReload(0.75f), Is.True);
            Assert.That(state.CompleteReloadIfReady(2.35f), Is.True);
            Assert.That(state.MagazineAmmo, Is.EqualTo(7));
            Assert.That(state.ReserveAmmo, Is.EqualTo(0));
        }

        [Test]
        public void FullMagazineCannotStartReload()
        {
            var state = CreateState(8, 24, 0.25f, 1.6f);

            Assert.That(state.TryStartReload(0f), Is.False);
            Assert.That(state.IsReloading, Is.False);
        }

        private static SemiAutomaticWeaponState CreateState(
            int capacity,
            int reserve,
            float shotInterval,
            float reloadDuration)
        {
            return new SemiAutomaticWeaponState(capacity, reserve, shotInterval, reloadDuration);
        }
    }
}
