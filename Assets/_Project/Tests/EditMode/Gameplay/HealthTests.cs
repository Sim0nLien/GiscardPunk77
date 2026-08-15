using GiscardPunk77.Core;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.Gameplay.Tests
{
    public sealed class HealthTests
    {
        private GameObject root;
        private Health health;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Health Test Target");
            health = root.AddComponent<Health>();
            health.ResetHealth();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void DamageInfoPreservesItsImmutableInputValues()
        {
            var source = new ActorIdentity(ActorKind.Player, new TeamId(7));
            var damage = new DamageInfo(34f, Vector3.one, Vector3.forward, source, DamageCategory.Hitscan);

            Assert.That(damage.Amount, Is.EqualTo(34f));
            Assert.That(damage.Point, Is.EqualTo(Vector3.one));
            Assert.That(damage.Direction, Is.EqualTo(Vector3.forward));
            Assert.That(damage.Source, Is.EqualTo(source));
            Assert.That(damage.Category, Is.EqualTo(DamageCategory.Hitscan));
        }

        [Test]
        public void NegativeAndZeroDamageAreIgnored()
        {
            var damageEvents = 0;
            health.Damaged += _ => damageEvents++;

            Assert.That(health.TryApplyDamage(CreateDamage(-1f)), Is.False);
            Assert.That(health.TryApplyDamage(CreateDamage(0f)), Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
            Assert.That(health.IsDead, Is.False);
            Assert.That(damageEvents, Is.EqualTo(0));
        }

        [Test]
        public void OverdamageClampsHealthToZero()
        {
            Assert.That(health.TryApplyDamage(CreateDamage(150f)), Is.True);

            Assert.That(health.CurrentHealth, Is.EqualTo(0f));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void ThreeThirtyFourDamageHitsKillOneHundredHealthExactlyOnce()
        {
            var damageEvents = 0;
            var deathEvents = 0;
            health.Damaged += _ => damageEvents++;
            health.Died += _ => deathEvents++;

            Assert.That(health.TryApplyDamage(CreateDamage(34f)), Is.True);
            Assert.That(health.TryApplyDamage(CreateDamage(34f)), Is.True);
            Assert.That(health.TryApplyDamage(CreateDamage(34f)), Is.True);
            Assert.That(health.TryApplyDamage(CreateDamage(34f)), Is.False);

            Assert.That(health.CurrentHealth, Is.EqualTo(0f));
            Assert.That(health.IsDead, Is.True);
            Assert.That(damageEvents, Is.EqualTo(3));
            Assert.That(deathEvents, Is.EqualTo(1));
        }

        [Test]
        public void HealingIsBoundedByMaximumHealth()
        {
            health.TryApplyDamage(CreateDamage(40f));

            Assert.That(health.TryHeal(100f), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
            Assert.That(health.TryHeal(1f), Is.False);
            Assert.That(health.TryHeal(0f), Is.False);
            Assert.That(health.TryHeal(-1f), Is.False);
        }

        [Test]
        public void ResetRestoresHealthAndStartsANewSingleDeathLifecycle()
        {
            var deathEvents = 0;
            health.Died += _ => deathEvents++;

            health.TryApplyDamage(CreateDamage(100f));
            health.ResetHealth();
            health.TryApplyDamage(CreateDamage(100f));

            Assert.That(health.CurrentHealth, Is.EqualTo(0f));
            Assert.That(health.IsDead, Is.True);
            Assert.That(deathEvents, Is.EqualTo(2));
        }

        [Test]
        public void HitboxDelegatesDamageToItsAssignedRootHealth()
        {
            var hitboxObject = new GameObject("Hitbox");
            hitboxObject.transform.SetParent(root.transform);
            var hitbox = hitboxObject.AddComponent<DamageableHitbox>();
            hitbox.AssignRootHealth(health);

            Assert.That(hitbox.TryApplyDamage(CreateDamage(34f)), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(66f));
            Assert.That(hitbox.RootHealth, Is.SameAs(health));
        }

        private static DamageInfo CreateDamage(float amount)
        {
            return new DamageInfo(
                amount,
                Vector3.zero,
                Vector3.forward,
                new ActorIdentity(ActorKind.Player, TeamId.Neutral),
                DamageCategory.Hitscan);
        }
    }
}
