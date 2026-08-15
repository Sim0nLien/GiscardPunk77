using System;
using UnityEngine;
using UnityEngine.AI;

namespace GiscardPunk77.AI.Navigation
{
    /// <summary>
    /// Owns the NavMeshAgent used by one NPC and exposes a small, observable movement API.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NpcMotor : MonoBehaviour
    {
        private const float ArrivalVelocityThresholdSquared = 0.01f;

        [SerializeField] private NavMeshAgent agent;
        [SerializeField, Min(0.05f)] private float destinationSampleDistance = 1f;
        [SerializeField, Min(0.1f)] private float stagnationCheckInterval = 0.5f;
        [SerializeField, Min(0.01f)] private float minimumProgressDistance = 0.05f;
        [SerializeField, Min(1)] private int stagnationChecksBeforeRecalculate = 3;
        [SerializeField, Min(0.1f)] private float recalculationCooldown = 0.5f;
        [SerializeField, Min(0)] private int maximumRecalculationAttempts = 2;
        [SerializeField, Min(1f)] private float manualRotationSpeed = 360f;

        private Vector3 destination;
        private Vector3 lastProgressPosition;
        private float lastRemainingDistance;
        private float nextStagnationCheckTime;
        private float nextRecalculationTime;
        private int recalculationAttempts;
        private int stagnationChecks;
        private bool hasActiveDestination;

        public NpcMotorStatus Status { get; private set; } = NpcMotorStatus.Idle;
        public NpcMotorFailureReason FailureReason { get; private set; } = NpcMotorFailureReason.None;
        public Vector3 Destination => destination;
        public bool HasActiveDestination => hasActiveDestination;
        public bool IsArrived => Status == NpcMotorStatus.Arrived;
        public bool IsPathPending => agent != null && agent.pathPending;
        public bool HasPath => agent != null && agent.hasPath;
        public NavMeshPathStatus PathStatus => agent != null ? agent.pathStatus : NavMeshPathStatus.PathInvalid;
        public float RemainingDistance => agent != null ? agent.remainingDistance : Mathf.Infinity;

        /// <summary>Raised only when the public status or its reason changes.</summary>
        public event Action<NpcMotorStatus, NpcMotorFailureReason> StatusChanged;

        private void Reset()
        {
            ResolveAgent();
            ApplyInitialAgentSettings();
        }

        private void Awake()
        {
            ResolveAgent();
        }

        private void OnEnable()
        {
            ResolveAgent();
            ClearRuntimeRoute();
            SetStatus(NpcMotorStatus.Idle, NpcMotorFailureReason.None);
        }

        private void OnDisable()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            ClearRuntimeRoute();
            SetStatus(NpcMotorStatus.Idle, NpcMotorFailureReason.None);
        }

        private void Update()
        {
            if (Status != NpcMotorStatus.Moving || agent == null || Time.time < nextStagnationCheckTime)
            {
                return;
            }

            nextStagnationCheckTime = Time.time + stagnationCheckInterval;

            if (!agent.isOnNavMesh)
            {
                Fail(NpcMotorFailureReason.NotOnNavMesh);
                return;
            }

            if (agent.pathPending)
            {
                return;
            }

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Fail(NpcMotorFailureReason.PathInvalid);
                return;
            }

            if (agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Fail(NpcMotorFailureReason.PathPartial);
                return;
            }

            if (HasReachedDestination())
            {
                SetStatus(NpcMotorStatus.Arrived, NpcMotorFailureReason.None);
                return;
            }

            var progressed = lastRemainingDistance - agent.remainingDistance >= minimumProgressDistance
                || (transform.position - lastProgressPosition).sqrMagnitude >= minimumProgressDistance * minimumProgressDistance;

            lastRemainingDistance = agent.remainingDistance;
            lastProgressPosition = transform.position;
            stagnationChecks = progressed ? 0 : stagnationChecks + 1;

            if (stagnationChecks >= stagnationChecksBeforeRecalculate)
            {
                TryRecalculate();
            }
        }

        /// <summary>Requests the nearest valid NavMesh point inside the configured bounded radius.</summary>
        public bool TrySetDestination(Vector3 worldDestination)
        {
            recalculationAttempts = 0;
            return TrySetDestinationInternal(worldDestination);
        }

        /// <summary>Pauses the current path without discarding its destination.</summary>
        public bool Stop()
        {
            if (!EnsureAgentOnNavMesh() || !hasActiveDestination)
            {
                return false;
            }

            agent.isStopped = true;
            SetStatus(NpcMotorStatus.Waiting, NpcMotorFailureReason.None);
            return true;
        }

        /// <summary>Continues a destination previously paused with Stop.</summary>
        public bool TryResume()
        {
            if (!EnsureAgentOnNavMesh() || !hasActiveDestination)
            {
                return false;
            }

            agent.isStopped = false;
            BeginProgressObservation();
            SetStatus(NpcMotorStatus.Moving, NpcMotorFailureReason.None);
            return true;
        }

        /// <summary>Rotates on the horizontal plane without moving the Transform position.</summary>
        public bool TryRotateTowards(Vector3 worldTarget)
        {
            var direction = worldTarget - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            var desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRotation,
                manualRotationSpeed * Time.deltaTime);
            return true;
        }

        /// <summary>Cancels the route and returns the motor to Idle.</summary>
        public void Cancel()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            ClearRuntimeRoute();
            SetStatus(NpcMotorStatus.Idle, NpcMotorFailureReason.None);
        }

        /// <summary>Retries the active destination, at most the configured number of times.</summary>
        public bool TryRecalculate()
        {
            if (!hasActiveDestination || !EnsureAgentOnNavMesh())
            {
                return false;
            }

            if (Time.time < nextRecalculationTime)
            {
                return false;
            }

            if (recalculationAttempts >= maximumRecalculationAttempts)
            {
                Fail(NpcMotorFailureReason.RecalculationLimitReached);
                return false;
            }

            recalculationAttempts++;
            nextRecalculationTime = Time.time + recalculationCooldown;
            agent.ResetPath();
            return TrySetDestinationInternal(destination);
        }

        /// <summary>Applies the P04 Humanoid defaults. Used by the sandbox authoring tool only.</summary>
        public void ApplyInitialAgentSettings()
        {
            ResolveAgent();
            if (agent == null)
            {
                return;
            }

            agent.agentTypeID = NpcSandboxTuning.HumanoidAgentTypeId;
            agent.radius = NpcSandboxTuning.AgentRadius;
            agent.height = NpcSandboxTuning.AgentHeight;
            agent.speed = NpcSandboxTuning.AgentSpeed;
            agent.angularSpeed = NpcSandboxTuning.AgentAngularSpeed;
            agent.acceleration = NpcSandboxTuning.AgentAcceleration;
            agent.stoppingDistance = NpcSandboxTuning.AgentStoppingDistance;
            agent.obstacleAvoidanceType = NpcSandboxTuning.AgentAvoidance;
            agent.avoidancePriority = NpcSandboxTuning.AgentAvoidancePriority;
            agent.autoTraverseOffMeshLink = true;
        }

        private bool TrySetDestinationInternal(Vector3 requestedDestination)
        {
            if (!EnsureAgentOnNavMesh())
            {
                return false;
            }

            if (!NavMesh.SamplePosition(requestedDestination, out var sampledDestination, destinationSampleDistance, NavMesh.AllAreas))
            {
                Fail(NpcMotorFailureReason.InvalidDestination);
                return false;
            }

            agent.isStopped = false;
            if (!agent.SetDestination(sampledDestination.position))
            {
                Fail(NpcMotorFailureReason.PathInvalid);
                return false;
            }

            destination = sampledDestination.position;
            hasActiveDestination = true;
            BeginProgressObservation();
            SetStatus(NpcMotorStatus.Moving, NpcMotorFailureReason.None);
            return true;
        }

        private bool EnsureAgentOnNavMesh()
        {
            ResolveAgent();
            if (agent == null)
            {
                Fail(NpcMotorFailureReason.MissingAgent);
                return false;
            }

            if (!isActiveAndEnabled || !agent.enabled)
            {
                Fail(NpcMotorFailureReason.Disabled);
                return false;
            }

            if (!agent.isOnNavMesh)
            {
                Fail(NpcMotorFailureReason.NotOnNavMesh);
                return false;
            }

            return true;
        }

        private bool HasReachedDestination()
        {
            return agent.remainingDistance <= agent.stoppingDistance
                && agent.velocity.sqrMagnitude <= ArrivalVelocityThresholdSquared;
        }

        private void BeginProgressObservation()
        {
            stagnationChecks = 0;
            lastProgressPosition = transform.position;
            lastRemainingDistance = agent.remainingDistance;
            nextStagnationCheckTime = Time.time + stagnationCheckInterval;
        }

        private void ClearRuntimeRoute()
        {
            hasActiveDestination = false;
            stagnationChecks = 0;
            recalculationAttempts = 0;
            nextStagnationCheckTime = 0f;
            nextRecalculationTime = 0f;
        }

        private void ResolveAgent()
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }
        }

        private void Fail(NpcMotorFailureReason reason)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            SetStatus(NpcMotorStatus.Failed, reason);
        }

        private void SetStatus(NpcMotorStatus status, NpcMotorFailureReason reason)
        {
            if (Status == status && FailureReason == reason)
            {
                return;
            }

            Status = status;
            FailureReason = reason;
            if (status == NpcMotorStatus.Failed)
            {
                Debug.LogWarning($"NpcMotor failed: {reason}.", this);
            }

            StatusChanged?.Invoke(status, reason);
        }
    }

    public enum NpcMotorStatus
    {
        Idle,
        Moving,
        Waiting,
        Arrived,
        Failed
    }

    public enum NpcMotorFailureReason
    {
        None,
        Disabled,
        MissingAgent,
        NotOnNavMesh,
        InvalidDestination,
        PathInvalid,
        PathPartial,
        Stuck,
        RecalculationLimitReached
    }
}
