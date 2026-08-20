using GiscardPunk77.AI.Behavior;
using GiscardPunk77.AI.Behavior.Guard;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.AI.Perception;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.AI.Tests
{
    public sealed class GuardContextTests
    {
        private GameObject guardObject;
        private GameObject serviceObject;
        private GuardConfig guardConfig;

        [TearDown]
        public void TearDown()
        {
            if (guardObject != null) Object.DestroyImmediate(guardObject);
            if (serviceObject != null) Object.DestroyImmediate(serviceObject);
            if (guardConfig != null) Object.DestroyImmediate(guardConfig);
        }

        [Test]
        public void AuthoredPatrolPointsStayRelativeToCapturedPost()
        {
            guardObject = new GameObject("Guard patrol route");
            var route = guardObject.AddComponent<GuardPatrolRoute>();
            route.Configure(new Vector3(0f, 0f, 2f), new Vector3(1f, 0f, 0f));
            route.Initialize(new Vector3(10f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f));

            Assert.That(route.TryGetWorldPoint(0, out var first), Is.True);
            Assert.That(first.x, Is.EqualTo(12f).Within(0.001f));
            Assert.That(first.z, Is.EqualTo(5f).Within(0.001f));
            Assert.That(route.TryGetWorldPoint(2, out _), Is.False);
        }

        [Test]
        public void TransitionHistoryKeepsOnlyConfiguredRecentEntries()
        {
            var context = CreateCompleteContext(out _);
            guardConfig.Configure(0f, 0f, 0f, 1f, 1f, 1f, 3);

            context.EnterState(GuardState.Patrol, "one");
            context.EnterState(GuardState.Suspicious, "two");
            context.EnterState(GuardState.InvestigateLastKnownPosition, "three");
            context.EnterState(GuardState.Idle, "four");

            Assert.That(context.TransitionHistory, Has.Count.EqualTo(3));
            Assert.That(context.TransitionHistory[0].Reason, Is.EqualTo("two"));
            Assert.That(context.TransitionHistory[2].To, Is.EqualTo(GuardState.Idle));
        }

        [Test]
        public void GlobalAlertImmediatelyCancelsRoutineAndRecordsTransition()
        {
            var context = CreateCompleteContext(out var alertService);
            context.EnterState(GuardState.Patrol, "test patrol");

            var raised = alertService.TryRaiseAlert(new AlertSnapshot(true, new Vector3(2f, 0f, 4f), 1f));

            Assert.That(raised, Is.True);
            Assert.That(context.CurrentState, Is.EqualTo(GuardState.GlobalAlerted));
            Assert.That(context.RequestedState, Is.EqualTo(GuardState.GlobalAlerted));
            Assert.That(context.TransitionHistory[^1].Reason, Does.Contain("Global alert"));
            Assert.That(context.Npc.Motor.Status, Is.EqualTo(NpcMotorStatus.Idle));
        }

        private GuardContext CreateCompleteContext(out AlertService alertService)
        {
            guardObject = new GameObject("Complete guard context");
            serviceObject = new GameObject("Scene alert service");
            alertService = serviceObject.AddComponent<AlertService>();
            guardConfig = ScriptableObject.CreateInstance<GuardConfig>();

            var identity = guardObject.AddComponent<ActorIdentityComponent>();
            identity.Configure(ActorKind.Guard, new TeamId(2));
            var health = guardObject.AddComponent<Health>();
            var motor = guardObject.AddComponent<NpcMotor>();
            var vision = guardObject.AddComponent<NpcVisionSensor>();
            var awareness = guardObject.AddComponent<NpcAwareness>();
            var npcContext = guardObject.AddComponent<NpcContext>();
            npcContext.Configure(identity, health, motor, vision, awareness, alertService);
            var route = guardObject.AddComponent<GuardPatrolRoute>();
            var context = guardObject.AddComponent<GuardContext>();
            context.Configure(npcContext, guardConfig, route);
            return context;
        }
    }
}
