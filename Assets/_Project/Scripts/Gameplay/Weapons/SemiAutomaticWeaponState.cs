using System;

namespace GiscardPunk77.Gameplay.Weapons
{
    /// <summary>
    /// Pure C# ammunition and timing rules. Presentation and input stay outside this class.
    /// </summary>
    public sealed class SemiAutomaticWeaponState
    {
        private const float MinimumDuration = 0.0001f;

        private float nextShotTime;
        private float reloadCompletionTime;

        public SemiAutomaticWeaponState(
            int magazineCapacity,
            int startingReserve,
            float shotInterval,
            float reloadDuration)
        {
            MagazineCapacity = Math.Max(1, magazineCapacity);
            MagazineAmmo = MagazineCapacity;
            ReserveAmmo = Math.Max(0, startingReserve);
            ShotInterval = Math.Max(MinimumDuration, shotInterval);
            ReloadDuration = Math.Max(MinimumDuration, reloadDuration);
        }

        public int MagazineCapacity { get; }
        public int MagazineAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }
        public float ShotInterval { get; }
        public float ReloadDuration { get; }
        public bool IsReloading { get; private set; }

        public bool TryConsumeShot(float time)
        {
            CompleteReloadIfReady(time);

            if (IsReloading || MagazineAmmo <= 0 || time < nextShotTime)
            {
                return false;
            }

            MagazineAmmo--;
            nextShotTime = time + ShotInterval;
            return true;
        }

        public bool TryStartReload(float time)
        {
            CompleteReloadIfReady(time);

            if (IsReloading || MagazineAmmo >= MagazineCapacity || ReserveAmmo <= 0)
            {
                return false;
            }

            IsReloading = true;
            reloadCompletionTime = time + ReloadDuration;
            return true;
        }

        public bool CompleteReloadIfReady(float time)
        {
            if (!IsReloading || time < reloadCompletionTime)
            {
                return false;
            }

            var transferredAmmo = Math.Min(MagazineCapacity - MagazineAmmo, ReserveAmmo);
            MagazineAmmo += transferredAmmo;
            ReserveAmmo -= transferredAmmo;
            IsReloading = false;
            return true;
        }
    }
}
