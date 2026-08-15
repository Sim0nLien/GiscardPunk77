using GiscardPunk77.AI.Perception;
using UnityEngine;

namespace GiscardPunk77.AI.Debugging
{
    /// <summary>Presentation-only billboard for the state owned by NpcAwareness.</summary>
    [DisallowMultipleComponent]
    public sealed class NpcAwarenessIndicator : MonoBehaviour
    {
        [SerializeField] private NpcAwareness awareness;
        [SerializeField] private NpcAwarenessConfig config;
        [SerializeField] private Camera presentationCamera;
        [SerializeField] private GameObject indicatorRoot;
        [SerializeField] private GameObject suspiciousSignal;
        [SerializeField] private GameObject alertSignal;

        private bool isSubscribed;

        private void Reset()
        {
            awareness = GetComponentInParent<NpcAwareness>();
            indicatorRoot = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            ApplyState(awareness != null ? awareness.State : NpcAwarenessState.Unaware);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void LateUpdate()
        {
            if (presentationCamera == null || indicatorRoot == null || !indicatorRoot.activeSelf)
            {
                return;
            }

            indicatorRoot.transform.rotation = presentationCamera.transform.rotation;
        }

        public void Configure(
            NpcAwareness awarenessSource,
            NpcAwarenessConfig awarenessConfig,
            Camera cameraSource,
            GameObject root,
            GameObject suspicious,
            GameObject alerted)
        {
            Unsubscribe();
            awareness = awarenessSource;
            config = awarenessConfig;
            presentationCamera = cameraSource;
            indicatorRoot = root;
            suspiciousSignal = suspicious;
            alertSignal = alerted;
            ResolveReferences();
            Subscribe();
            ApplyState(awareness != null ? awareness.State : NpcAwarenessState.Unaware);
        }

        private void ResolveReferences()
        {
            if (awareness == null)
            {
                awareness = GetComponentInParent<NpcAwareness>();
            }

            if (config == null && awareness != null)
            {
                config = awareness.Config;
            }

            if (indicatorRoot == null)
            {
                indicatorRoot = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
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

        private void OnAwarenessStateChanged(
            NpcAwarenessState previousState,
            NpcAwarenessState nextState)
        {
            ApplyState(nextState);
        }

        private void ApplyState(NpcAwarenessState awarenessState)
        {
            var showRoot = awarenessState != NpcAwarenessState.Unaware;
            if (indicatorRoot != null)
            {
                indicatorRoot.SetActive(showRoot);
                if (showRoot && config != null)
                {
                    indicatorRoot.transform.localPosition = Vector3.up * config.IndicatorHeight;
                    indicatorRoot.transform.localScale = Vector3.one * config.IndicatorScale;
                }
            }

            if (suspiciousSignal != null)
            {
                suspiciousSignal.SetActive(
                    awarenessState == NpcAwarenessState.Suspicious
                    && config != null
                    && config.ShowSuspicionIndicator);
            }

            if (alertSignal != null)
            {
                alertSignal.SetActive(awarenessState == NpcAwarenessState.Alerted);
            }
        }
    }
}
