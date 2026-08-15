using GiscardPunk77.AI.Perception;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.AI.Tests
{
    public sealed class NpcAwarenessTests
    {
        private GameObject runtimeObject;
        private NpcAwarenessConfig config;

        [TearDown]
        public void TearDown()
        {
            if (runtimeObject != null)
            {
                Object.DestroyImmediate(runtimeObject);
            }

            if (config != null)
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void HysteresisPreventsOscillationAroundThresholds()
        {
            var tuning = CreateConfig().Tuning;

            var state = NpcAwarenessModel.EvaluateState(NpcAwarenessState.Unaware, 0.25f, tuning);
            Assert.That(state, Is.EqualTo(NpcAwarenessState.Suspicious));

            state = NpcAwarenessModel.EvaluateState(state, 0.2f, tuning);
            Assert.That(state, Is.EqualTo(NpcAwarenessState.Suspicious));

            state = NpcAwarenessModel.EvaluateState(state, 0.12f, tuning);
            Assert.That(state, Is.EqualTo(NpcAwarenessState.Unaware));

            state = NpcAwarenessModel.EvaluateState(NpcAwarenessState.Suspicious, 0.85f, tuning);
            Assert.That(state, Is.EqualTo(NpcAwarenessState.Alerted));

            state = NpcAwarenessModel.EvaluateState(state, 0.7f, tuning);
            Assert.That(state, Is.EqualTo(NpcAwarenessState.Alerted));
        }

        [Test]
        public void SuspicionDecaysAndStaysNormalized()
        {
            var tuning = CreateConfig().Tuning;

            var decayed = NpcAwarenessModel.AdvanceSuspicion(0.5f, 1f, false, 0f, tuning);
            var fullyDecayed = NpcAwarenessModel.AdvanceSuspicion(decayed, 99f, false, 0f, tuning);

            Assert.That(decayed, Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(fullyDecayed, Is.Zero);
        }

        [Test]
        public void LastSeenPositionAndTimeFreezeWhenSightIsLost()
        {
            var awareness = CreateAwareness();
            var seenPoint = new Vector3(2f, 1f, 5f);

            awareness.Observe(CreateObservation(true, seenPoint, 12f, 1f));
            awareness.Advance(0.5f);
            awareness.Observe(CreateObservation(false, new Vector3(-5f, 1f, 1f), 20f, 0f));
            awareness.Advance(0.5f);

            Assert.That(awareness.HasLastSeenPosition, Is.True);
            Assert.That(awareness.LastSeenPosition, Is.EqualTo(seenPoint));
            Assert.That(awareness.LastSeenTime, Is.EqualTo(12f));
            Assert.That(awareness.Suspicion, Is.LessThan(0.5f));
        }

        [Test]
        public void AlertIsPublishedOncePerContinuousAcquisition()
        {
            var awareness = CreateAwareness();
            var alertTransitions = 0;
            awareness.StateChanged += (_, next) =>
            {
                if (next == NpcAwarenessState.Alerted)
                {
                    alertTransitions++;
                }
            };

            awareness.Observe(CreateObservation(true, Vector3.forward, 1f, 1f));
            awareness.Advance(1f);
            awareness.Advance(1f);

            Assert.That(awareness.State, Is.EqualTo(NpcAwarenessState.Alerted));
            Assert.That(alertTransitions, Is.EqualTo(1));
        }

        [Test]
        public void ResetClearsSuspicionStateAndMemory()
        {
            var awareness = CreateAwareness();
            awareness.Observe(CreateObservation(true, new Vector3(3f, 1f, 4f), 4f, 1f));
            awareness.Advance(1f);

            awareness.ResetAwareness();

            Assert.That(awareness.Suspicion, Is.Zero);
            Assert.That(awareness.State, Is.EqualTo(NpcAwarenessState.Unaware));
            Assert.That(awareness.HasLastSeenPosition, Is.False);
            Assert.That(float.IsNegativeInfinity(awareness.LastSeenTime), Is.True);
        }

        private NpcAwarenessConfig CreateConfig()
        {
            config = ScriptableObject.CreateInstance<NpcAwarenessConfig>();
            config.Configure(1f, 0.2f, 0.25f, 0.12f, 0.85f, 0.6f, true, 2.25f, 0.7f);
            return config;
        }

        private NpcAwareness CreateAwareness()
        {
            runtimeObject = new GameObject("NpcAwareness Tests");
            var awareness = runtimeObject.AddComponent<NpcAwareness>();
            awareness.Configure(CreateConfig(), null);
            return awareness;
        }

        private static NpcVisionObservation CreateObservation(
            bool hasLineOfSight,
            Vector3 targetPoint,
            float sampleTime,
            float detectionProgress)
        {
            var score = new NpcVisionScore(
                1f,
                0f,
                hasLineOfSight ? 1f : 0f,
                hasLineOfSight ? 1f : 0f,
                1f,
                hasLineOfSight,
                hasLineOfSight);
            return new NpcVisionObservation(
                true,
                Vector3.zero,
                targetPoint,
                sampleTime,
                false,
                score,
                false,
                detectionProgress,
                1f);
        }
    }
}
