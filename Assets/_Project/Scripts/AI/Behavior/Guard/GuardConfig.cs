using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Guard
{
    [CreateAssetMenu(fileName = "GuardConfig", menuName = "GiscardPunk77/AI/Guard Config")]
    public sealed class GuardConfig : ScriptableObject
    {
        [Header("Routine")]
        [SerializeField, Min(0f)] private float idleSeconds = 1.5f;
        [SerializeField, Min(0f)] private float suspiciousOrientationSeconds = 0.8f;
        [SerializeField, Min(0f)] private float investigationWaitSeconds = 2f;
        [SerializeField, Min(0.1f)] private float investigationTimeoutSeconds = 8f;
        [SerializeField, Min(0.1f)] private float returnToPostTimeoutSeconds = 8f;
        [SerializeField, Min(0.1f)] private float lastKnownPositionLifetimeSeconds = 6f;

        [Header("Diagnostics")]
        [SerializeField, Range(2, 16)] private int transitionHistoryCapacity = 8;
        [SerializeField] private Color idleColor = new(0.25f, 0.55f, 1f);
        [SerializeField] private Color patrolColor = new(0.25f, 0.9f, 0.35f);
        [SerializeField] private Color suspiciousColor = new(1f, 0.75f, 0.1f);
        [SerializeField] private Color investigateColor = new(1f, 0.35f, 0.1f);
        [SerializeField] private Color globalAlertColor = new(0.9f, 0.05f, 0.05f);

        public float IdleSeconds => idleSeconds;
        public float SuspiciousOrientationSeconds => suspiciousOrientationSeconds;
        public float InvestigationWaitSeconds => investigationWaitSeconds;
        public float InvestigationTimeoutSeconds => investigationTimeoutSeconds;
        public float ReturnToPostTimeoutSeconds => returnToPostTimeoutSeconds;
        public float LastKnownPositionLifetimeSeconds => lastKnownPositionLifetimeSeconds;
        public int TransitionHistoryCapacity => transitionHistoryCapacity;

        public Color GetColor(GuardState state)
        {
            return state switch
            {
                GuardState.Patrol => patrolColor,
                GuardState.Suspicious => suspiciousColor,
                GuardState.InvestigateLastKnownPosition => investigateColor,
                GuardState.GlobalAlerted => globalAlertColor,
                _ => idleColor
            };
        }

        public void Configure(
            float idleDuration,
            float suspiciousOrientationDuration,
            float investigationWaitDuration,
            float investigationTimeout,
            float returnTimeout,
            float lastKnownLifetime,
            int historyCapacity)
        {
            idleSeconds = Mathf.Max(0f, idleDuration);
            suspiciousOrientationSeconds = Mathf.Max(0f, suspiciousOrientationDuration);
            investigationWaitSeconds = Mathf.Max(0f, investigationWaitDuration);
            investigationTimeoutSeconds = Mathf.Max(0.1f, investigationTimeout);
            returnToPostTimeoutSeconds = Mathf.Max(0.1f, returnTimeout);
            lastKnownPositionLifetimeSeconds = Mathf.Max(0.1f, lastKnownLifetime);
            transitionHistoryCapacity = Mathf.Clamp(historyCapacity, 2, 16);
        }

        private void OnValidate()
        {
            Configure(
                idleSeconds,
                suspiciousOrientationSeconds,
                investigationWaitSeconds,
                investigationTimeoutSeconds,
                returnToPostTimeoutSeconds,
                lastKnownPositionLifetimeSeconds,
                transitionHistoryCapacity);
        }
    }
}
