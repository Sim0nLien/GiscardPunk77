using System;
using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    /// <summary>
    /// Converts observations into normalized suspicion and short-term memory.
    /// It owns neither raycasts nor UI presentation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcAwareness : MonoBehaviour
    {
        [SerializeField] private NpcAwarenessConfig config;
        [SerializeField] private NpcVisionSensor visionSensor;

        [Header("Runtime diagnostic (read only)")]
        [SerializeField, Range(0f, 1f)] private float suspicion;
        [SerializeField] private NpcAwarenessState state;
        [SerializeField] private bool hasLastSeenPosition;
        [SerializeField] private Vector3 lastSeenPosition;
        [SerializeField] private float lastSeenTime = float.NegativeInfinity;

        private bool isSeeingTarget;
        private float latestDetectionProgress;
        private bool isSubscribed;

        public NpcAwarenessConfig Config => config;
        public float Suspicion => suspicion;
        public NpcAwarenessState State => state;
        public bool HasLastSeenPosition => hasLastSeenPosition;
        public Vector3 LastSeenPosition => lastSeenPosition;
        public float LastSeenTime => lastSeenTime;

        public event Action<NpcAwarenessState, NpcAwarenessState> StateChanged;
        public event Action<float> SuspicionChanged;

        private void Reset()
        {
            ResolveVisionSensor();
        }

        private void Awake()
        {
            ResolveVisionSensor();
        }

        private void OnEnable()
        {
            ResolveVisionSensor();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        public void Configure(NpcAwarenessConfig awarenessConfig, NpcVisionSensor sensor)
        {
            Unsubscribe();
            config = awarenessConfig;
            visionSensor = sensor;
            Subscribe();
            ResetAwareness();
        }

        /// <summary>Consumes a sensor result without taking ownership of sensing.</summary>
        public void Observe(NpcVisionObservation observation)
        {
            isSeeingTarget = observation.HasLineOfSight;
            latestDetectionProgress = observation.DetectionProgress;

            if (!isSeeingTarget)
            {
                return;
            }

            hasLastSeenPosition = true;
            lastSeenPosition = observation.TargetPoint;
            lastSeenTime = observation.SampleTime;
        }

        /// <summary>Advances suspicion with a caller-provided duration to stay testable.</summary>
        public void Advance(float elapsedSeconds)
        {
            if (config == null)
            {
                return;
            }

            var previousSuspicion = suspicion;
            suspicion = NpcAwarenessModel.AdvanceSuspicion(
                suspicion,
                elapsedSeconds,
                isSeeingTarget,
                latestDetectionProgress,
                config.Tuning);

            if (!Mathf.Approximately(previousSuspicion, suspicion))
            {
                SuspicionChanged?.Invoke(suspicion);
            }

            SetState(NpcAwarenessModel.EvaluateState(state, suspicion, config.Tuning));
        }

        [ContextMenu("P08/Reset Awareness")]
        public void ResetAwareness()
        {
            var previousState = state;
            suspicion = 0f;
            state = NpcAwarenessState.Unaware;
            isSeeingTarget = false;
            latestDetectionProgress = 0f;
            hasLastSeenPosition = false;
            lastSeenPosition = Vector3.zero;
            lastSeenTime = float.NegativeInfinity;
            SuspicionChanged?.Invoke(suspicion);

            if (previousState != state)
            {
                StateChanged?.Invoke(previousState, state);
            }
        }

        private void ResolveVisionSensor()
        {
            if (visionSensor == null)
            {
                visionSensor = GetComponent<NpcVisionSensor>();
            }
        }

        private void Subscribe()
        {
            if (isSubscribed || visionSensor == null)
            {
                return;
            }

            visionSensor.ObservationUpdated += Observe;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || visionSensor == null)
            {
                isSubscribed = false;
                return;
            }

            visionSensor.ObservationUpdated -= Observe;
            isSubscribed = false;
        }

        private void SetState(NpcAwarenessState nextState)
        {
            if (state == nextState)
            {
                return;
            }

            var previousState = state;
            state = nextState;
            StateChanged?.Invoke(previousState, nextState);
        }
    }
}
