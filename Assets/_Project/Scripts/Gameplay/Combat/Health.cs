using System;
using UnityEngine;

namespace GiscardPunk77.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        private const float MinimumMaxHealth = 0.01f;

        [SerializeField, Min(MinimumMaxHealth)]
        private float maxHealth = 100f;

        private float currentHealth;
        private bool isDead;

        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Died;

        public float MaxHealth => maxHealth;

        public float CurrentHealth => currentHealth;

        public bool IsDead => isDead;

        private void Awake()
        {
            maxHealth = Mathf.Max(MinimumMaxHealth, maxHealth);
            ResetHealth();
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(MinimumMaxHealth, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public bool TryApplyDamage(in DamageInfo damage)
        {
            if (isDead || !IsPositiveAmount(damage.Amount))
            {
                return false;
            }

            var previousHealth = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth - damage.Amount, 0f, maxHealth);

            if (currentHealth >= previousHealth)
            {
                return false;
            }

            Damaged?.Invoke(damage);

            if (currentHealth > 0f)
            {
                return true;
            }

            isDead = true;
            Died?.Invoke(damage);
            return true;
        }

        public bool TryHeal(float amount)
        {
            if (isDead || !IsPositiveAmount(amount))
            {
                return false;
            }

            var previousHealth = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            return currentHealth > previousHealth;
        }

        public void ResetHealth()
        {
            maxHealth = Mathf.Max(MinimumMaxHealth, maxHealth);
            currentHealth = maxHealth;
            isDead = false;
        }

        private static bool IsPositiveAmount(float amount)
        {
            return amount > 0f && !float.IsNaN(amount);
        }
    }
}
