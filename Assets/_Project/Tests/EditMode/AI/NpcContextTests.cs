using GiscardPunk77.AI.Behavior;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.AI.Perception;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.AI.Tests
{
    public sealed class NpcContextTests
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
        public void MissingReferencesProduceExplicitDiagnostic()
        {
            npc = new GameObject("Incomplete NPC context");
            var context = npc.AddComponent<NpcContext>();

            var isValid = context.TryValidate(
                NpcContextRequirement.Health | NpcContextRequirement.Motor,
                out var error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Does.Contain("Health"));
            Assert.That(error, Does.Contain("Motor"));
            Assert.That(context.LastValidationError, Is.EqualTo(error));
        }

        [Test]
        public void ExplicitlyConfiguredContextExposesEveryDependency()
        {
            npc = new GameObject("Complete NPC context");
            serviceObject = new GameObject("Scene alert service");
            var identity = npc.AddComponent<ActorIdentityComponent>();
            identity.Configure(ActorKind.Guard, new TeamId(2));
            var health = npc.AddComponent<Health>();
            var motor = npc.AddComponent<NpcMotor>();
            var vision = npc.AddComponent<NpcVisionSensor>();
            var awareness = npc.AddComponent<NpcAwareness>();
            var alertService = serviceObject.AddComponent<AlertService>();
            var context = npc.AddComponent<NpcContext>();

            context.Configure(identity, health, motor, vision, awareness, alertService);

            Assert.That(context.TryValidate(out var error), Is.True, error);
            Assert.That(context.Identity.Identity.Kind, Is.EqualTo(ActorKind.Guard));
            Assert.That(context.Health, Is.SameAs(health));
            Assert.That(context.Motor, Is.SameAs(motor));
            Assert.That(context.Vision, Is.SameAs(vision));
            Assert.That(context.Awareness, Is.SameAs(awareness));
            Assert.That(context.AlertService, Is.SameAs(alertService));
        }
    }
}
