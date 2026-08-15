using GiscardPunk77.Core;
using UnityEngine;

namespace GiscardPunk77.Gameplay
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            float amount,
            Vector3 point,
            Vector3 direction,
            ActorIdentity source,
            DamageCategory category)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Source = source;
            Category = category;
        }

        public float Amount { get; }

        public Vector3 Point { get; }

        public Vector3 Direction { get; }

        public ActorIdentity Source { get; }

        public DamageCategory Category { get; }
    }
}
