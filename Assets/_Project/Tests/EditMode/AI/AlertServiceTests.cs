using System.Collections.Generic;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Perception;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.AI.Tests
{
    public sealed class AlertServiceTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void SubscriptionBroadcastsOnceAndUnsubscriptionStopsUpdates()
        {
            var service = CreateService();
            var notifications = 0;
            System.Action<AlertLevel, AlertSnapshot> listener = (_, __) => notifications++;
            service.LevelChanged += listener;

            Assert.That(service.TryRaiseAlert(AlertSnapshot.None), Is.True);
            Assert.That(service.TryRaiseAlert(new AlertSnapshot(true, Vector3.one, 2f)), Is.False);
            Assert.That(notifications, Is.EqualTo(1));

            service.LevelChanged -= listener;
            Assert.That(service.ResetAlert(), Is.True);
            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void NewReadersSeeCurrentLevelAndFrozenInitialSnapshot()
        {
            var service = CreateService();
            var firstPoint = new Vector3(4f, 1f, 8f);
            Assert.That(service.TryRaiseAlert(new AlertSnapshot(true, firstPoint, 12f)), Is.True);

            var newGuardReader = service;

            Assert.That(newGuardReader.Level, Is.EqualTo(AlertLevel.Alerted));
            Assert.That(newGuardReader.Snapshot.HasInitialObservation, Is.True);
            Assert.That(newGuardReader.Snapshot.InitialObservationPoint, Is.EqualTo(firstPoint));
            Assert.That(newGuardReader.Snapshot.InitialObservationTime, Is.EqualTo(12f));
        }

        [Test]
        public void ResetPublishesCalmOnceAndClearsSnapshot()
        {
            var service = CreateService();
            var transitions = new List<AlertLevel>();
            service.LevelChanged += (level, _) => transitions.Add(level);

            service.TryRaiseAlert(new AlertSnapshot(true, Vector3.forward, 3f));
            Assert.That(service.ResetAlert(), Is.True);
            Assert.That(service.ResetAlert(), Is.False);

            Assert.That(transitions, Is.EqualTo(new[] { AlertLevel.Alerted, AlertLevel.Calm }));
            Assert.That(service.Level, Is.EqualTo(AlertLevel.Calm));
            Assert.That(service.Snapshot.HasInitialObservation, Is.False);
            Assert.That(float.IsNegativeInfinity(service.Snapshot.InitialObservationTime), Is.True);
        }

        [Test]
        public void AlertedGuardReportsOwnFrozenLastSeenSnapshot()
        {
            var service = CreateService();
            var guard = new GameObject("Alert reporter test guard");
            createdObjects.Add(guard);
            var config = ScriptableObject.CreateInstance<NpcAwarenessConfig>();
            createdObjects.Add(config);
            config.Configure(1f, 0.2f, 0.25f, 0.12f, 0.85f, 0.6f, true, 2.25f, 0.7f);
            var awareness = guard.AddComponent<NpcAwareness>();
            awareness.Configure(config, null);
            var reporter = guard.AddComponent<NpcAlertReporter>();
            reporter.Configure(awareness, service);
            var seenPoint = new Vector3(7f, 1f, 2f);

            awareness.Observe(CreateVisibleObservation(seenPoint, 9f));
            awareness.Advance(1f);

            Assert.That(service.Level, Is.EqualTo(AlertLevel.Alerted));
            Assert.That(service.Snapshot.InitialObservationPoint, Is.EqualTo(seenPoint));
            Assert.That(service.Snapshot.InitialObservationTime, Is.EqualTo(9f));
        }

        private AlertService CreateService()
        {
            var serviceObject = new GameObject("Alert service tests");
            createdObjects.Add(serviceObject);
            return serviceObject.AddComponent<AlertService>();
        }

        private static NpcVisionObservation CreateVisibleObservation(Vector3 targetPoint, float sampleTime)
        {
            var score = new NpcVisionScore(1f, 0f, 1f, 1f, 1f, true, true);
            return new NpcVisionObservation(true, Vector3.zero, targetPoint, sampleTime, false, score, false, 1f, 1f);
        }
    }
}
