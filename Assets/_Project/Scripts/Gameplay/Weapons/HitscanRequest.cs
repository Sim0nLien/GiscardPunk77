using GiscardPunk77.Core;
using UnityEngine;

namespace GiscardPunk77.Gameplay.Weapons
{
    public readonly struct HitscanRequest
    {
        public HitscanRequest(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            LayerMask layerMask,
            Transform ignoredRoot,
            float damage,
            ActorIdentity source,
            DamageCategory category = DamageCategory.Hitscan,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            Origin = origin;
            Direction = direction;
            MaxDistance = maxDistance;
            LayerMask = layerMask;
            IgnoredRoot = ignoredRoot;
            Damage = damage;
            Source = source;
            Category = category;
            TriggerInteraction = triggerInteraction;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float MaxDistance { get; }
        public LayerMask LayerMask { get; }
        public Transform IgnoredRoot { get; }
        public float Damage { get; }
        public ActorIdentity Source { get; }
        public DamageCategory Category { get; }
        public QueryTriggerInteraction TriggerInteraction { get; }
    }
}
