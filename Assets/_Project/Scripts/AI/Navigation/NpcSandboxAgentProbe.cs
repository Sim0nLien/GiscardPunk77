using UnityEngine;

namespace GiscardPunk77.AI.Navigation
{
    /// <summary>
    /// Sandbox-only route probe. It consumes NpcMotor and never controls NavMeshAgent directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcSandboxAgentProbe : MonoBehaviour
    {
        [SerializeField] private NpcMotor motor;
        [SerializeField] private Transform[] destinations;
        [SerializeField, Min(0f)] private float waitAtDestination = 1f;
        [SerializeField] private bool cycleAutomatically = true;

        private int destinationIndex;
        private float waitElapsed;
        private bool destinationRequested;
        private bool failureReported;

        public int DestinationCount => destinations != null ? destinations.Length : 0;

        private void Reset()
        {
            ResolveMotor();
        }

        private void Awake()
        {
            ResolveMotor();
        }

        private void OnEnable()
        {
            destinationIndex = 0;
            waitElapsed = 0f;
            destinationRequested = false;
            failureReported = false;
        }

        private void Update()
        {
            if (!cycleAutomatically || motor == null || DestinationCount == 0)
            {
                return;
            }

            if (!destinationRequested)
            {
                RequestCurrentDestination();
                return;
            }

            if (motor.Status == NpcMotorStatus.Failed)
            {
                ReportPathFailureOnce();
                return;
            }

            if (!motor.IsArrived)
            {
                return;
            }

            waitElapsed += Time.deltaTime;
            if (waitElapsed < waitAtDestination)
            {
                return;
            }

            destinationIndex = (destinationIndex + 1) % DestinationCount;
            waitElapsed = 0f;
            destinationRequested = false;
            failureReported = false;
        }

        public void Configure(NpcMotor routeMotor, Transform[] routeDestinations)
        {
            motor = routeMotor;
            destinations = routeDestinations;
            destinationIndex = 0;
            destinationRequested = false;
            failureReported = false;
            ResolveMotor();
        }

        private void RequestCurrentDestination()
        {
            var destination = destinations[destinationIndex];
            if (destination == null || !motor.TrySetDestination(destination.position))
            {
                ReportPathFailureOnce();
                return;
            }

            destinationRequested = true;
        }

        private void ReportPathFailureOnce()
        {
            if (failureReported)
            {
                return;
            }

            failureReported = true;
            Debug.LogWarning(
                $"P04 navigation probe cannot reach destination index {destinationIndex}. " +
                "Verify the NavMesh bake and the door threshold link.",
                this);
        }

        private void ResolveMotor()
        {
            if (motor == null)
            {
                motor = GetComponent<NpcMotor>();
            }
        }
    }
}
