using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    public enum NpcAwarenessState
    {
        Unaware,
        Suspicious,
        Alerted
    }

    /// <summary>Pure suspicion and hysteresis rules, independent from sensors and presentation.</summary>
    public static class NpcAwarenessModel
    {
        public static float AdvanceSuspicion(
            float currentSuspicion,
            float elapsedSeconds,
            bool hasLineOfSight,
            float detectionProgress,
            NpcAwarenessTuning tuning)
        {
            var boundedCurrent = Mathf.Clamp01(currentSuspicion);
            var elapsed = Mathf.Max(0f, elapsedSeconds);
            if (!hasLineOfSight)
            {
                return Mathf.Clamp01(boundedCurrent - tuning.SuspicionDecayPerSecond * elapsed);
            }

            var gain = tuning.SuspicionGainPerSecond * Mathf.Clamp01(detectionProgress);
            return Mathf.Clamp01(boundedCurrent + gain * elapsed);
        }

        public static NpcAwarenessState EvaluateState(
            NpcAwarenessState currentState,
            float suspicion,
            NpcAwarenessTuning tuning)
        {
            var value = Mathf.Clamp01(suspicion);
            switch (currentState)
            {
                case NpcAwarenessState.Alerted:
                    if (value >= tuning.AlertedExitThreshold)
                    {
                        return NpcAwarenessState.Alerted;
                    }

                    return value >= tuning.SuspiciousEnterThreshold
                        ? NpcAwarenessState.Suspicious
                        : NpcAwarenessState.Unaware;

                case NpcAwarenessState.Suspicious:
                    if (value >= tuning.AlertedEnterThreshold)
                    {
                        return NpcAwarenessState.Alerted;
                    }

                    return value > tuning.SuspiciousExitThreshold
                        ? NpcAwarenessState.Suspicious
                        : NpcAwarenessState.Unaware;

                default:
                    if (value >= tuning.AlertedEnterThreshold)
                    {
                        return NpcAwarenessState.Alerted;
                    }

                    return value >= tuning.SuspiciousEnterThreshold
                        ? NpcAwarenessState.Suspicious
                        : NpcAwarenessState.Unaware;
            }
        }
    }
}
