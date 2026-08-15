using System;
using System.Collections.Generic;
using UnityEngine;

namespace GiscardPunk77.Gameplay.Weapons
{
    /// <summary>
    /// Resolves one instantaneous shot. The first non-shooter collider always blocks the ray,
    /// whether it can receive damage or not.
    /// </summary>
    public sealed class HitscanResolver
    {
        private static readonly IComparer<RaycastHit> HitDistanceComparer =
            Comparer<RaycastHit>.Create((left, right) => left.distance.CompareTo(right.distance));

        public HitscanResult Resolve(in HitscanRequest request)
        {
            if (request.MaxDistance <= 0f || request.Direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return HitscanResult.Miss;
            }

            var hits = Physics.RaycastAll(
                request.Origin,
                request.Direction.normalized,
                request.MaxDistance,
                request.LayerMask,
                request.TriggerInteraction);

            Array.Sort(hits, HitDistanceComparer);

            foreach (var hit in hits)
            {
                if (IsInIgnoredHierarchy(hit.transform, request.IgnoredRoot))
                {
                    continue;
                }

                var damageable = FindDamageable(hit.collider);
                var damageApplied = false;

                if (damageable != null)
                {
                    var damage = new DamageInfo(
                        request.Damage,
                        hit.point,
                        request.Direction.normalized,
                        request.Source,
                        request.Category);
                    damageApplied = damageable.TryApplyDamage(damage);
                }

                return new HitscanResult(true, hit, damageApplied);
            }

            return HitscanResult.Miss;
        }

        private static bool IsInIgnoredHierarchy(Transform candidate, Transform ignoredRoot)
        {
            return ignoredRoot != null
                && candidate != null
                && (candidate == ignoredRoot || candidate.IsChildOf(ignoredRoot));
        }

        private static IDamageable FindDamageable(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            var behaviours = collider.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IDamageable damageable)
                {
                    return damageable;
                }
            }

            return null;
        }
    }
}
