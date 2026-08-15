using UnityEngine;

namespace GiscardPunk77.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class DamageableHitbox : MonoBehaviour, IDamageable
    {
        [SerializeField]
        private Health rootHealth;

        public Health RootHealth => rootHealth;

        public bool IsDead => rootHealth != null && rootHealth.IsDead;

        private void Awake()
        {
            ResolveRootHealthIfNeeded();
        }

        private void OnValidate()
        {
            ResolveRootHealthIfNeeded();
        }

        private void Reset()
        {
            ResolveRootHealthIfNeeded();
        }

        public bool TryApplyDamage(in DamageInfo damage)
        {
            return rootHealth != null && rootHealth.TryApplyDamage(damage);
        }

        public void AssignRootHealth(Health health)
        {
            rootHealth = health;
        }

        private void ResolveRootHealthIfNeeded()
        {
            if (rootHealth == null)
            {
                rootHealth = GetComponentInParent<Health>();
            }
        }
    }
}
