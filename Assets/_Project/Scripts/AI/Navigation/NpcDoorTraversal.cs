using System;
using GiscardPunk77.Gameplay;
using GiscardPunk77.Gameplay.Doors;
using UnityEngine;

namespace GiscardPunk77.AI.Navigation
{
    /// <summary>
    /// Coordinates one NPC through an IDoorPassage without knowing the concrete door implementation.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcMotor))]
    public sealed class NpcDoorTraversal : MonoBehaviour
    {
        [SerializeField] private NpcMotor motor;
        [SerializeField] private MonoBehaviour doorPassageSource;
        [SerializeField] private Health health;
        [SerializeField] private Transform clearancePointA;
        [SerializeField] private Transform clearancePointB;
        [SerializeField] private bool beginOnStart;
        [SerializeField, Min(1f)] private float traversalTimeout = 30f;

        private IDoorPassage door;
        private Transform approachPoint;
        private Transform exitPoint;
        private float deadline;
        private bool healthSubscribed;

        public NpcDoorTraversalState State { get; private set; } = NpcDoorTraversalState.Idle;

        public NpcDoorTraversalFailureReason FailureReason { get; private set; }

        public bool IsActive => State != NpcDoorTraversalState.Idle
            && State != NpcDoorTraversalState.Completed
            && State != NpcDoorTraversalState.Failed;

        public event Action<NpcDoorTraversalState, NpcDoorTraversalFailureReason> StateChanged;

        private void Reset()
        {
            motor = GetComponent<NpcMotor>();
            health = GetComponent<Health>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToHealth();
            if (State == NpcDoorTraversalState.Failed
                && FailureReason == NpcDoorTraversalFailureReason.Disabled)
            {
                SetState(NpcDoorTraversalState.Idle, NpcDoorTraversalFailureReason.None);
            }
        }

        private void Start()
        {
            if (beginOnStart)
            {
                BeginTraversal();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromHealth();
            ReleaseReservation();
            if (IsActive)
            {
                motor?.Cancel();
                SetState(NpcDoorTraversalState.Failed, NpcDoorTraversalFailureReason.Disabled);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromHealth();
            ReleaseReservation();
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            if (Time.time >= deadline)
            {
                Fail(NpcDoorTraversalFailureReason.Timeout);
                return;
            }

            if (door == null || !door.CanUse)
            {
                Fail(NpcDoorTraversalFailureReason.DoorUnavailable);
                return;
            }

            switch (State)
            {
                case NpcDoorTraversalState.WaitingForReservation:
                    UpdateReservationWait();
                    break;
                case NpcDoorTraversalState.MovingToWaitingPoint:
                    UpdateApproach();
                    break;
                case NpcDoorTraversalState.WaitingForPassableDoor:
                    UpdateDoorWait();
                    break;
                case NpcDoorTraversalState.Traversing:
                    UpdateCrossing();
                    break;
            }
        }

        public void Configure(
            NpcMotor traversalMotor,
            MonoBehaviour passageSource,
            Health traversalHealth,
            bool autoBegin,
            float timeout = 30f)
        {
            UnsubscribeFromHealth();
            ReleaseReservation();
            motor = traversalMotor;
            doorPassageSource = passageSource;
            health = traversalHealth;
            beginOnStart = autoBegin;
            traversalTimeout = Mathf.Max(1f, timeout);
            ResolveReferences();
            SubscribeToHealth();
            SetState(NpcDoorTraversalState.Idle, NpcDoorTraversalFailureReason.None);
        }

        /// <summary>
        /// Defines agent-specific destinations that clear the shared waiting points before release.
        /// Missing points keep the legacy waiting-point destinations for backwards compatibility.
        /// </summary>
        public void ConfigureClearancePoints(Transform pointA, Transform pointB)
        {
            clearancePointA = pointA;
            clearancePointB = pointB;
        }

        public bool BeginTraversal()
        {
            if (IsActive)
            {
                return false;
            }

            ResolveReferences();
            if (motor == null)
            {
                Fail(NpcDoorTraversalFailureReason.MissingMotor);
                return false;
            }

            if (door == null)
            {
                Fail(NpcDoorTraversalFailureReason.MissingDoor);
                return false;
            }

            if (!door.CanUse)
            {
                Fail(NpcDoorTraversalFailureReason.DoorUnavailable);
                return false;
            }

            if (!TrySelectPassagePoints())
            {
                Fail(NpcDoorTraversalFailureReason.MissingWaitingPoints);
                return false;
            }

            motor.Cancel();
            deadline = Time.time + traversalTimeout;
            if (door.TryReserve(this))
            {
                return BeginApproach();
            }

            SetState(NpcDoorTraversalState.WaitingForReservation, NpcDoorTraversalFailureReason.None);
            return true;
        }

        public void ResetTraversal()
        {
            ReleaseReservation();
            motor?.Cancel();
            approachPoint = null;
            exitPoint = null;
            deadline = 0f;
            SetState(NpcDoorTraversalState.Idle, NpcDoorTraversalFailureReason.None);
        }

        private void UpdateReservationWait()
        {
            if (door.TryReserve(this))
            {
                BeginApproach();
            }
        }

        private void UpdateApproach()
        {
            if (!door.TryReserve(this))
            {
                motor.Cancel();
                SetState(NpcDoorTraversalState.WaitingForReservation, NpcDoorTraversalFailureReason.None);
                return;
            }

            if (motor.Status == NpcMotorStatus.Failed)
            {
                Fail(NpcDoorTraversalFailureReason.MotorRejectedPath);
                return;
            }

            if (!motor.IsArrived)
            {
                return;
            }

            if (!door.RequestOpen())
            {
                Fail(NpcDoorTraversalFailureReason.DoorUnavailable);
                return;
            }

            SetState(NpcDoorTraversalState.WaitingForPassableDoor, NpcDoorTraversalFailureReason.None);
        }

        private void UpdateDoorWait()
        {
            if (!door.TryReserve(this))
            {
                motor.Cancel();
                SetState(NpcDoorTraversalState.WaitingForReservation, NpcDoorTraversalFailureReason.None);
                return;
            }

            door.RequestOpen();
            if (!door.IsPassable)
            {
                return;
            }

            if (!motor.TrySetDestination(exitPoint.position))
            {
                Fail(NpcDoorTraversalFailureReason.MotorRejectedPath);
                return;
            }

            SetState(NpcDoorTraversalState.Traversing, NpcDoorTraversalFailureReason.None);
        }

        private void UpdateCrossing()
        {
            if (!door.TryReserve(this))
            {
                Fail(NpcDoorTraversalFailureReason.ReservationLost);
                return;
            }

            door.RequestOpen();
            if (motor.Status == NpcMotorStatus.Failed)
            {
                Fail(NpcDoorTraversalFailureReason.MotorRejectedPath);
                return;
            }

            if (!motor.IsArrived)
            {
                return;
            }

            ReleaseReservation();
            SetState(NpcDoorTraversalState.Completed, NpcDoorTraversalFailureReason.None);
        }

        private bool BeginApproach()
        {
            if (!motor.TrySetDestination(approachPoint.position))
            {
                Fail(NpcDoorTraversalFailureReason.MotorRejectedPath);
                return false;
            }

            SetState(NpcDoorTraversalState.MovingToWaitingPoint, NpcDoorTraversalFailureReason.None);
            return true;
        }

        private bool TrySelectPassagePoints()
        {
            var pointA = door.WaitingPointA;
            var pointB = door.WaitingPointB;
            if (pointA == null || pointB == null || pointA == pointB)
            {
                return false;
            }

            var distanceToA = (transform.position - pointA.position).sqrMagnitude;
            var distanceToB = (transform.position - pointB.position).sqrMagnitude;
            if (distanceToA <= distanceToB)
            {
                approachPoint = pointA;
                exitPoint = clearancePointB != null ? clearancePointB : pointB;
            }
            else
            {
                approachPoint = pointB;
                exitPoint = clearancePointA != null ? clearancePointA : pointA;
            }

            return true;
        }

        private void ResolveReferences()
        {
            if (motor == null)
            {
                motor = GetComponent<NpcMotor>();
            }

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            door = doorPassageSource as IDoorPassage;
        }

        private void SubscribeToHealth()
        {
            if (!isActiveAndEnabled || health == null || healthSubscribed)
            {
                return;
            }

            health.Died += OnDied;
            healthSubscribed = true;
        }

        private void UnsubscribeFromHealth()
        {
            if (health == null || !healthSubscribed)
            {
                return;
            }

            health.Died -= OnDied;
            healthSubscribed = false;
        }

        private void OnDied(DamageInfo damage)
        {
            if (IsActive)
            {
                Fail(NpcDoorTraversalFailureReason.Died);
            }
            else
            {
                ReleaseReservation();
            }
        }

        private void Fail(NpcDoorTraversalFailureReason reason)
        {
            ReleaseReservation();
            motor?.Cancel();
            SetState(NpcDoorTraversalState.Failed, reason);
            Debug.LogWarning($"NpcDoorTraversal failed: {reason}.", this);
        }

        private void ReleaseReservation()
        {
            door?.Release(this);
        }

        private void SetState(NpcDoorTraversalState state, NpcDoorTraversalFailureReason reason)
        {
            if (State == state && FailureReason == reason)
            {
                return;
            }

            State = state;
            FailureReason = reason;
            StateChanged?.Invoke(state, reason);
        }
    }

    public enum NpcDoorTraversalState
    {
        Idle,
        WaitingForReservation,
        MovingToWaitingPoint,
        WaitingForPassableDoor,
        Traversing,
        Completed,
        Failed
    }

    public enum NpcDoorTraversalFailureReason
    {
        None,
        MissingMotor,
        MissingDoor,
        MissingWaitingPoints,
        DoorUnavailable,
        MotorRejectedPath,
        ReservationLost,
        Timeout,
        Died,
        Disabled
    }
}
