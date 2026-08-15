using UnityEngine;

namespace GiscardPunk77.Gameplay.Weapons
{
    public readonly struct HitscanResult
    {
        public HitscanResult(bool hasHit, RaycastHit hit, bool damageApplied)
        {
            HasHit = hasHit;
            Hit = hit;
            DamageApplied = damageApplied;
        }

        public bool HasHit { get; }
        public RaycastHit Hit { get; }
        public bool DamageApplied { get; }

        public static HitscanResult Miss => new HitscanResult(false, default, false);
    }
}
