using System.Collections.Generic;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay.Weapons;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.Gameplay.Tests
{
    public sealed class HitscanResolverTests
    {
        private readonly List<GameObject> objectsToDestroy = new List<GameObject>();
        private readonly HitscanResolver resolver = new HitscanResolver();
        private Vector3 origin;

        [SetUp]
        public void SetUp()
        {
            origin = new Vector3(10000f, 10000f, 10000f);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var gameObject in objectsToDestroy)
            {
                Object.DestroyImmediate(gameObject);
            }

            objectsToDestroy.Clear();
        }

        [Test]
        public void ShooterHierarchyIsIgnoredAndTargetBehindItReceivesDamage()
        {
            var shooter = CreateObject("Shooter", origin + Vector3.forward * 2f);
            CreateBox("Shooter Child Collider", shooter.transform.position, shooter.transform);
            var target = CreateHealthTarget("Target", origin + Vector3.forward * 5f);
            Physics.SyncTransforms();

            var result = Resolve(shooter.transform, ~0);

            Assert.That(result.HasHit, Is.True);
            Assert.That(result.DamageApplied, Is.True);
            Assert.That(target.CurrentHealth, Is.EqualTo(66f));
        }

        [Test]
        public void FirstWallBlocksTargetBehindIt()
        {
            var wall = CreateBox("Wall", origin + Vector3.forward * 3f);
            var target = CreateHealthTarget("Target Behind Wall", origin + Vector3.forward * 6f);
            Physics.SyncTransforms();

            var result = Resolve(null, ~0);

            Assert.That(result.HasHit, Is.True);
            Assert.That(result.Hit.collider, Is.SameAs(wall));
            Assert.That(result.DamageApplied, Is.False);
            Assert.That(target.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void TargetBeforeWallReceivesDamage()
        {
            var target = CreateHealthTarget("Target Before Wall", origin + Vector3.forward * 3f);
            CreateBox("Wall", origin + Vector3.forward * 6f);
            Physics.SyncTransforms();

            var result = Resolve(null, ~0);

            Assert.That(result.HasHit, Is.True);
            Assert.That(result.DamageApplied, Is.True);
            Assert.That(target.CurrentHealth, Is.EqualTo(66f));
        }

        [Test]
        public void LayerMaskCanExcludeACloserObstacle()
        {
            var ignoredWall = CreateBox("Ignored Wall", origin + Vector3.forward * 3f);
            ignoredWall.gameObject.layer = 2;
            var target = CreateHealthTarget("Included Target", origin + Vector3.forward * 6f);
            Physics.SyncTransforms();

            var defaultLayerOnly = 1 << 0;
            var result = Resolve(null, defaultLayerOnly);

            Assert.That(result.DamageApplied, Is.True);
            Assert.That(target.CurrentHealth, Is.EqualTo(66f));
        }

        [Test]
        public void ChildHitboxDelegatesDamageToRootHealth()
        {
            var root = CreateObject("Target Root", origin + Vector3.forward * 5f);
            var health = root.AddComponent<Health>();
            health.ResetHealth();

            var hitboxObject = CreateObject("Child Hitbox", root.transform.position, root.transform);
            var collider = hitboxObject.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
            var hitbox = hitboxObject.AddComponent<DamageableHitbox>();
            hitbox.AssignRootHealth(health);
            Physics.SyncTransforms();

            var result = Resolve(null, ~0);

            Assert.That(result.Hit.collider, Is.SameAs(collider));
            Assert.That(result.DamageApplied, Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(66f));
        }

        private HitscanResult Resolve(Transform ignoredRoot, LayerMask layers)
        {
            var request = new HitscanRequest(
                origin,
                Vector3.forward,
                20f,
                layers,
                ignoredRoot,
                34f,
                new ActorIdentity(ActorKind.Player, TeamId.Neutral));
            return resolver.Resolve(request);
        }

        private Health CreateHealthTarget(string name, Vector3 position)
        {
            var target = CreateObject(name, position);
            target.AddComponent<BoxCollider>();
            var health = target.AddComponent<Health>();
            health.ResetHealth();
            return health;
        }

        private BoxCollider CreateBox(string name, Vector3 position, Transform parent = null)
        {
            var gameObject = CreateObject(name, position, parent);
            return gameObject.AddComponent<BoxCollider>();
        }

        private GameObject CreateObject(string name, Vector3 position, Transform parent = null)
        {
            var gameObject = new GameObject(name);
            objectsToDestroy.Add(gameObject);
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            return gameObject;
        }
    }
}
