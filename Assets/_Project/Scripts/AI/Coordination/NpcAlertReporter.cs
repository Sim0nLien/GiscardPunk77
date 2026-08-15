using GiscardPunk77.AI.Perception;
using UnityEngine;

namespace GiscardPunk77.AI.Coordination
{
    /// <summary>
    /// Bridges one guard's awareness to an explicitly assigned scene AlertService.
    /// It reports only its own frozen last-seen values; it never reads a player Transform.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcAlertReporter : MonoBehaviour
    {
        [SerializeField] private NpcAwareness awareness;
        [SerializeField] private AlertService alertService;

        private bool isSubscribed;

        private void Awake()
        {
            ResolveAwareness();
        }

        private void OnEnable()
        {
            ResolveAwareness();
            Subscribe();
            ReportCurrentAlert();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(NpcAwareness sourceAwareness, AlertService sceneAlertService)
        {
            Unsubscribe();
            awareness = sourceAwareness;
            alertService = sceneAlertService;
            Subscribe();
            ReportCurrentAlert();
        }

        private void ResolveAwareness()
        {
            if (awareness == null)
            {
                awareness = GetComponent<NpcAwareness>();
            }
        }

        private void Subscribe()
        {
            if (isSubscribed || awareness == null)
            {
                return;
            }

            awareness.StateChanged += OnAwarenessStateChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (isSubscribed && awareness != null)
            {
                awareness.StateChanged -= OnAwarenessStateChanged;
            }

            isSubscribed = false;
        }

        private void OnAwarenessStateChanged(NpcAwarenessState _, NpcAwarenessState next)
        {
            if (next == NpcAwarenessState.Alerted)
            {
                ReportCurrentAlert();
            }
        }

        private void ReportCurrentAlert()
        {
            if (awareness == null || alertService == null || awareness.State != NpcAwarenessState.Alerted)
            {
                return;
            }

            var snapshot = awareness.HasLastSeenPosition
                ? new AlertSnapshot(true, awareness.LastSeenPosition, awareness.LastSeenTime)
                : AlertSnapshot.None;
            alertService.TryRaiseAlert(snapshot);
        }
    }
}
