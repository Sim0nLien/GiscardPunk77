using System;
using System.Collections.Generic;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Perception;
using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Guard
{
    /// <summary>Guard-specific authored data and short runtime diagnostics layered over NpcContext.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GuardPatrolRoute))]
    public sealed class GuardContext : MonoBehaviour
    {
        [Header("Explicit references")]
        [SerializeField] private NpcContext npc;
        [SerializeField] private GuardConfig config;
        [SerializeField] private GuardPatrolRoute patrolRoute;

        [Header("Runtime diagnostic (read only)")]
        [SerializeField] private GuardState currentState = GuardState.Idle;
        [SerializeField] private GuardState requestedState = GuardState.Idle;
        [SerializeField] private Vector3 postPosition;
        [SerializeField] private int patrolPointIndex;
        [SerializeField] private List<GuardTransitionRecord> transitionHistory = new();
        [SerializeField] private string lastValidationError;

        private Quaternion postRotation = Quaternion.identity;
        private AlertService subscribedAlertService;

        public NpcContext Npc => npc;
        public GuardConfig Config => config;
        public GuardPatrolRoute PatrolRoute => patrolRoute;
        public GuardState CurrentState => currentState;
        public GuardState RequestedState => requestedState;
        public Vector3 PostPosition => postPosition;
        public int PatrolPointIndex => patrolPointIndex;
        public IReadOnlyList<GuardTransitionRecord> TransitionHistory => transitionHistory;
        public string LastValidationError => lastValidationError;
        public bool IsGloballyAlerted => npc != null && npc.IsGloballyAlerted;
        public NpcAwareness Awareness => npc != null ? npc.Awareness : null;

        public event Action<GuardState, GuardState> StateChanged;

        private void Reset()
        {
            patrolRoute = GetComponent<GuardPatrolRoute>();
            npc = GetComponent<NpcContext>();
        }

        private void Awake()
        {
            ResolveLocalReferences();
            CapturePost();
        }

        private void OnEnable()
        {
            ResolveLocalReferences();
            SubscribeAlertService();
        }

        private void OnDisable()
        {
            UnsubscribeAlertService();
        }

        public void Configure(NpcContext npcContext, GuardConfig guardConfig, GuardPatrolRoute route)
        {
            UnsubscribeAlertService();
            npc = npcContext;
            config = guardConfig;
            patrolRoute = route;
            CapturePost();
            SubscribeAlertService();
        }

        public bool TryValidate(out string error)
        {
            var missing = new List<string>(3);
            if (npc == null) missing.Add(nameof(Npc));
            if (config == null) missing.Add(nameof(Config));
            if (patrolRoute == null) missing.Add(nameof(PatrolRoute));

            if (missing.Count > 0)
            {
                error = $"{name}: GuardContext is missing required reference(s): {string.Join(", ", missing)}.";
                lastValidationError = error;
                return false;
            }

            var requirements = NpcContextRequirement.Motor |
                NpcContextRequirement.Awareness |
                NpcContextRequirement.AlertService;
            if (!npc.TryValidate(requirements, out error))
            {
                lastValidationError = error;
                return false;
            }

            lastValidationError = string.Empty;
            return true;
        }

        public bool Require(UnityEngine.Object requester = null)
        {
            if (TryValidate(out _))
            {
                return true;
            }

            Debug.LogError(lastValidationError, requester != null ? requester : this);
            return false;
        }

        public void RequestState(GuardState state)
        {
            requestedState = state;
        }

        public void EnterState(GuardState state, string reason)
        {
            requestedState = state;
            if (currentState == state)
            {
                return;
            }

            var previous = currentState;
            currentState = state;
            transitionHistory.Add(new GuardTransitionRecord(Time.time, previous, state, reason ?? string.Empty));
            var capacity = config != null ? config.TransitionHistoryCapacity : 8;
            while (transitionHistory.Count > capacity)
            {
                transitionHistory.RemoveAt(0);
            }

            StateChanged?.Invoke(previous, state);
        }

        public bool TryGetCurrentPatrolPoint(out Vector3 point)
        {
            point = default;
            return patrolRoute != null && patrolRoute.TryGetWorldPoint(patrolPointIndex, out point);
        }

        public void AdvancePatrolPoint()
        {
            patrolPointIndex = patrolRoute == null || patrolRoute.Count == 0
                ? 0
                : (patrolPointIndex + 1) % patrolRoute.Count;
        }

        public bool TryGetLastKnownPosition(out Vector3 position)
        {
            var awareness = Awareness;
            if (awareness == null || !awareness.HasLastSeenPosition || config == null)
            {
                position = default;
                return false;
            }

            if (Time.time - awareness.LastSeenTime > config.LastKnownPositionLifetimeSeconds)
            {
                position = default;
                return false;
            }

            position = awareness.LastSeenPosition;
            return true;
        }

        [ContextMenu("P11/Reset Guard Routine")]
        public void ResetRoutine()
        {
            npc?.Motor?.Cancel();
            patrolPointIndex = 0;
            transitionHistory.Clear();
            currentState = GuardState.Idle;
            requestedState = GuardState.Idle;
            CapturePost();
            StateChanged?.Invoke(GuardState.Idle, GuardState.Idle);
        }

        private void CapturePost()
        {
            postPosition = transform.position;
            postRotation = transform.rotation;
            patrolRoute?.Initialize(postPosition, postRotation);
        }

        private void ResolveLocalReferences()
        {
            npc ??= GetComponent<NpcContext>();
            patrolRoute ??= GetComponent<GuardPatrolRoute>();
        }

        private void SubscribeAlertService()
        {
            var service = npc != null ? npc.AlertService : null;
            if (service == null || subscribedAlertService == service)
            {
                return;
            }

            UnsubscribeAlertService();
            subscribedAlertService = service;
            subscribedAlertService.LevelChanged += OnAlertLevelChanged;
            if (subscribedAlertService.IsAlerted)
            {
                InterruptForGlobalAlert();
            }
        }

        private void UnsubscribeAlertService()
        {
            if (subscribedAlertService != null)
            {
                subscribedAlertService.LevelChanged -= OnAlertLevelChanged;
            }

            subscribedAlertService = null;
        }

        private void OnAlertLevelChanged(AlertLevel level, AlertSnapshot snapshot)
        {
            if (level == AlertLevel.Alerted)
            {
                InterruptForGlobalAlert();
            }
        }

        private void InterruptForGlobalAlert()
        {
            npc?.Motor?.Cancel();
            EnterState(GuardState.GlobalAlerted, "Global alert interrupted routine");
        }
    }
}
