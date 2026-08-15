using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    [CreateAssetMenu(
        fileName = "NpcAwarenessConfig",
        menuName = "GiscardPunk77/AI/NPC Awareness Config")]
    public sealed class NpcAwarenessConfig : ScriptableObject
    {
        [Header("Suspicion")]
        [SerializeField, Min(0f)] private float suspicionGainPerSecond = 0.9f;
        [SerializeField, Min(0f)] private float suspicionDecayPerSecond = 0.2f;

        [Header("Hysteresis")]
        [SerializeField, Range(0.01f, 0.99f)] private float suspiciousEnterThreshold = 0.25f;
        [SerializeField, Range(0f, 0.98f)] private float suspiciousExitThreshold = 0.12f;
        [SerializeField, Range(0.02f, 1f)] private float alertedEnterThreshold = 0.85f;
        [SerializeField, Range(0.01f, 0.99f)] private float alertedExitThreshold = 0.6f;

        [Header("Presentation")]
        [SerializeField] private bool showSuspicionIndicator = true;
        [SerializeField, Min(0.1f)] private float indicatorHeight = 2.25f;
        [SerializeField, Min(0.1f)] private float indicatorScale = 0.7f;

        public bool ShowSuspicionIndicator => showSuspicionIndicator;
        public float IndicatorHeight => indicatorHeight;
        public float IndicatorScale => indicatorScale;
        public NpcAwarenessTuning Tuning => new NpcAwarenessTuning(
            suspicionGainPerSecond,
            suspicionDecayPerSecond,
            suspiciousEnterThreshold,
            suspiciousExitThreshold,
            alertedEnterThreshold,
            alertedExitThreshold);

        private void OnValidate()
        {
            suspicionGainPerSecond = Mathf.Max(0f, suspicionGainPerSecond);
            suspicionDecayPerSecond = Mathf.Max(0f, suspicionDecayPerSecond);
            suspiciousEnterThreshold = Mathf.Clamp(suspiciousEnterThreshold, 0.01f, 0.97f);
            suspiciousExitThreshold = Mathf.Clamp(suspiciousExitThreshold, 0f, suspiciousEnterThreshold - 0.01f);
            alertedEnterThreshold = Mathf.Clamp(alertedEnterThreshold, suspiciousEnterThreshold + 0.01f, 1f);
            alertedExitThreshold = Mathf.Clamp(
                alertedExitThreshold,
                suspiciousEnterThreshold,
                alertedEnterThreshold - 0.01f);
            indicatorHeight = Mathf.Max(0.1f, indicatorHeight);
            indicatorScale = Mathf.Max(0.1f, indicatorScale);
        }

        public void Configure(
            float gainPerSecond,
            float decayPerSecond,
            float suspiciousEnter,
            float suspiciousExit,
            float alertedEnter,
            float alertedExit,
            bool showSuspicion,
            float height,
            float scale)
        {
            suspicionGainPerSecond = gainPerSecond;
            suspicionDecayPerSecond = decayPerSecond;
            suspiciousEnterThreshold = suspiciousEnter;
            suspiciousExitThreshold = suspiciousExit;
            alertedEnterThreshold = alertedEnter;
            alertedExitThreshold = alertedExit;
            showSuspicionIndicator = showSuspicion;
            indicatorHeight = height;
            indicatorScale = scale;
            OnValidate();
        }
    }

    /// <summary>Validated immutable values consumed by the awareness model.</summary>
    public readonly struct NpcAwarenessTuning
    {
        public NpcAwarenessTuning(
            float gainPerSecond,
            float decayPerSecond,
            float suspiciousEnter,
            float suspiciousExit,
            float alertedEnter,
            float alertedExit)
        {
            SuspicionGainPerSecond = Mathf.Max(0f, gainPerSecond);
            SuspicionDecayPerSecond = Mathf.Max(0f, decayPerSecond);
            SuspiciousEnterThreshold = Mathf.Clamp(suspiciousEnter, 0.01f, 0.97f);
            SuspiciousExitThreshold = Mathf.Clamp(suspiciousExit, 0f, SuspiciousEnterThreshold - 0.01f);
            AlertedEnterThreshold = Mathf.Clamp(alertedEnter, SuspiciousEnterThreshold + 0.01f, 1f);
            AlertedExitThreshold = Mathf.Clamp(
                alertedExit,
                SuspiciousEnterThreshold,
                AlertedEnterThreshold - 0.01f);
        }

        public float SuspicionGainPerSecond { get; }
        public float SuspicionDecayPerSecond { get; }
        public float SuspiciousEnterThreshold { get; }
        public float SuspiciousExitThreshold { get; }
        public float AlertedEnterThreshold { get; }
        public float AlertedExitThreshold { get; }
    }
}
