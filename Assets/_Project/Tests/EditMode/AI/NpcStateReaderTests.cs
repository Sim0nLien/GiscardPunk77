using GiscardPunk77.AI.Behavior;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.AI.Tests
{
    public sealed class NpcStateReaderTests
    {
        private GameObject npc;
        private GameObject serviceObject;

        [TearDown]
        public void TearDown()
        {
            if (npc != null)
            {
                Object.DestroyImmediate(npc);
            }

            if (serviceObject != null)
            {
                Object.DestroyImmediate(serviceObject);
            }
        }

        [Test]
        public void MissingDependencyReturnsDiagnosticInsteadOfGuessingState()
        {
            npc = new GameObject("State reader without awareness");
            var context = npc.AddComponent<NpcContext>();

            var valid = NpcStateReader.TryMatches(
                context,
                NpcStateQuery.AwarenessSuspicious,
                out var matches,
                out var error);

            Assert.That(valid, Is.False);
            Assert.That(matches, Is.False);
            Assert.That(error, Does.Contain("Awareness"));
        }

        [Test]
        public void AlertQueryReadsSceneServiceWithoutFollowingATarget()
        {
            var context = CreateHealthMotorAndAlertContext(out _);

            AssertMatch(context, NpcStateQuery.GlobalCalm, true);
            context.AlertService.TryRaiseAlert(new AlertSnapshot(true, new Vector3(2f, 0f, 3f), 4f));
            AssertMatch(context, NpcStateQuery.GlobalAlerted, true);
            AssertMatch(context, NpcStateQuery.DeadOrGloballyAlerted, true);
        }

        [Test]
        public void DeathAndMotorQueriesReadTheirOwningComponents()
        {
            var context = CreateHealthMotorAndAlertContext(out var health);

            AssertMatch(context, NpcStateQuery.Alive, true);
            AssertMatch(context, NpcStateQuery.MotorIdle, true);

            var lethalDamage = new DamageInfo(
                health.MaxHealth,
                Vector3.zero,
                Vector3.forward,
                default(ActorIdentity),
                default(DamageCategory));
            health.TryApplyDamage(lethalDamage);

            AssertMatch(context, NpcStateQuery.Dead, true);
            AssertMatch(context, NpcStateQuery.DeadOrGloballyAlerted, true);
        }

        private NpcContext CreateHealthMotorAndAlertContext(out Health health)
        {
            npc = new GameObject("State reader NPC");
            serviceObject = new GameObject("State reader alert service");
            health = npc.AddComponent<Health>();
            var motor = npc.AddComponent<NpcMotor>();
            var alertService = serviceObject.AddComponent<AlertService>();
            var context = npc.AddComponent<NpcContext>();
            context.Configure(null, health, motor, null, null, alertService);
            return context;
        }

        private static void AssertMatch(NpcContext context, NpcStateQuery query, bool expected)
        {
            var valid = NpcStateReader.TryMatches(context, query, out var matches, out var error);
            Assert.That(valid, Is.True, error);
            Assert.That(matches, Is.EqualTo(expected));
        }
    }
}
