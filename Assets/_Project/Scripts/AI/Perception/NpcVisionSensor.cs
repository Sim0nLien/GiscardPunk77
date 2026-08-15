using System;
using GiscardPunk77.Core;
using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    /// <summary>
    /// Samples one visibility target and publishes immutable observations.
    /// It deliberately owns no suspicion, alert or guard state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcVisionSensor : MonoBehaviour
    {
        private const int ConeSegments = 24;

        [Header("References")]
        [SerializeField] private Transform eye;
        [SerializeField] private MonoBehaviour targetSource;

        [Header("Vision")]
        [SerializeField, Min(0.1f)] private float maximumDistance = 12f;
        [SerializeField, Range(1f, 179f)] private float fieldOfViewDegrees = 100f;
        [SerializeField, Min(0.01f)] private float standingExposureSeconds = 0.6f;
        [SerializeField, Range(0.05f, 1f)] private float crouchingDistanceMultiplier = 0.65f;
        [SerializeField, Min(1f)] private float crouchingExposureMultiplier = 1.75f;
        [SerializeField] private LayerMask occlusionMask = ~0;

        [Header("Sampling")]
        [SerializeField, Min(0.1f)] private float samplesPerSecond = 8f;
        [SerializeField] private bool drawGizmos = true;

        [Header("Runtime diagnostic (read only)")]
        [SerializeField] private float lastDetectionProgress;
        [SerializeField] private bool lastRayBlocked;
        [SerializeField] private bool lastTargetDetected;

        private IVisibilityTarget target;
        private Transform targetTransform;
        private float exposureSeconds;
        private float nextSampleTime;
        private Vector3 lastRayOrigin;
        private Vector3 lastRayEnd;
        private Vector3 lastBlockingPoint;
        private bool hasSampledRay;
        private bool lastRayWasCandidate;

        public NpcVisionObservation LastObservation { get; private set; }
        public float SampleInterval => 1f / Mathf.Max(0.1f, samplesPerSecond);
        public bool HasValidTarget => target != null;

        public event Action<NpcVisionObservation> ObservationUpdated;

        private NpcVisionParameters Parameters => new NpcVisionParameters(
            maximumDistance,
            fieldOfViewDegrees,
            standingExposureSeconds,
            crouchingDistanceMultiplier,
            crouchingExposureMultiplier);

        private void Reset()
        {
            eye = transform;
        }

        private void Awake()
        {
            ResolveTarget();
        }

        private void OnEnable()
        {
            ResolveTarget();
            ResetExposure();
            nextSampleTime = Time.time + NpcVisionEvaluation.CalculateInitialSampleDelay(
                GetEntityId().GetHashCode(),
                SampleInterval);
        }

        private void OnDisable()
        {
            ResetExposure();
        }

        private void OnValidate()
        {
            maximumDistance = Mathf.Max(0.1f, maximumDistance);
            fieldOfViewDegrees = Mathf.Clamp(fieldOfViewDegrees, 1f, 179f);
            standingExposureSeconds = Mathf.Max(0.01f, standingExposureSeconds);
            crouchingDistanceMultiplier = Mathf.Clamp(crouchingDistanceMultiplier, 0.05f, 1f);
            crouchingExposureMultiplier = Mathf.Max(1f, crouchingExposureMultiplier);
            samplesPerSecond = Mathf.Max(0.1f, samplesPerSecond);
        }

        private void Update()
        {
            if (Time.time < nextSampleTime)
            {
                return;
            }

            SampleNow();
            nextSampleTime = Time.time + SampleInterval;
        }

        public void Configure(
            Transform eyeTransform,
            MonoBehaviour visibilityTarget,
            LayerMask blockingLayers,
            float distance,
            float viewAngle,
            float exposureDuration,
            float crouchingDistance,
            float crouchingExposure,
            float samplingFrequency)
        {
            eye = eyeTransform != null ? eyeTransform : transform;
            targetSource = visibilityTarget;
            occlusionMask = blockingLayers;
            maximumDistance = distance;
            fieldOfViewDegrees = viewAngle;
            standingExposureSeconds = exposureDuration;
            crouchingDistanceMultiplier = crouchingDistance;
            crouchingExposureMultiplier = crouchingExposure;
            samplesPerSecond = samplingFrequency;
            OnValidate();
            ResolveTarget();
            ResetExposure();
        }

        public bool TrySetTarget(MonoBehaviour visibilityTarget)
        {
            targetSource = visibilityTarget;
            ResolveTarget();
            ResetExposure();
            return target != null;
        }

        public void ClearTarget()
        {
            targetSource = null;
            target = null;
            targetTransform = null;
            ResetExposure();
        }

        [ContextMenu("P07/Reset Exposure")]
        public void ResetExposure()
        {
            exposureSeconds = 0f;
            lastDetectionProgress = 0f;
            lastTargetDetected = false;
        }

        /// <summary>Performs one bounded sample. Useful for explicit probes and tests.</summary>
        public NpcVisionObservation SampleNow()
        {
            var observer = eye != null ? eye : transform;
            var observerPoint = observer.position;

            if (targetSource == null)
            {
                target = null;
                targetTransform = null;
            }

            if (target == null)
            {
                exposureSeconds = 0f;
                hasSampledRay = false;
                lastRayBlocked = false;
                lastTargetDetected = false;
                lastDetectionProgress = 0f;
                LastObservation = default;
                ObservationUpdated?.Invoke(LastObservation);
                return LastObservation;
            }

            var targetPoint = target.VisibilityPoint;
            var isCrouching = target.IsCrouching;
            var parameters = Parameters;
            var score = NpcVisionEvaluation.EvaluateGeometry(
                observerPoint,
                observer.forward,
                targetPoint,
                isCrouching,
                parameters);
            var isOccluded = TestOcclusion(observerPoint, targetPoint, score.IsCandidate);
            exposureSeconds = NpcVisionEvaluation.AdvanceExposure(
                exposureSeconds,
                SampleInterval,
                score,
                isOccluded,
                isCrouching,
                parameters);
            var requiredExposure = parameters.GetRequiredExposure(isCrouching);

            LastObservation = new NpcVisionObservation(
                true,
                observerPoint,
                targetPoint,
                Time.time,
                isCrouching,
                score,
                isOccluded,
                exposureSeconds,
                requiredExposure);
            lastDetectionProgress = LastObservation.DetectionProgress;
            lastRayBlocked = isOccluded;
            lastTargetDetected = LastObservation.IsDetected;
            ObservationUpdated?.Invoke(LastObservation);
            return LastObservation;
        }

        private void ResolveTarget()
        {
            target = targetSource as IVisibilityTarget;
            targetTransform = targetSource != null ? targetSource.transform : null;

            if (targetSource != null && target == null)
            {
                Debug.LogWarning(
                    $"{targetSource.name} must implement {nameof(IVisibilityTarget)} to be observed.",
                    this);
            }
        }

        private bool TestOcclusion(Vector3 origin, Vector3 targetPoint, bool shouldRaycast)
        {
            lastRayOrigin = origin;
            lastRayEnd = targetPoint;
            lastBlockingPoint = targetPoint;
            hasSampledRay = true;
            lastRayWasCandidate = shouldRaycast;

            if (!shouldRaycast)
            {
                return false;
            }

            var toTarget = targetPoint - origin;
            var distance = toTarget.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            if (!Physics.Raycast(
                    origin,
                    toTarget / distance,
                    out var hit,
                    distance,
                    occlusionMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (IsPartOfTarget(hit.transform))
            {
                return false;
            }

            lastBlockingPoint = hit.point;
            return true;
        }

        private bool IsPartOfTarget(Transform hitTransform)
        {
            if (targetTransform == null || hitTransform == null)
            {
                return false;
            }

            return hitTransform == targetTransform
                || hitTransform.IsChildOf(targetTransform)
                || targetTransform.IsChildOf(hitTransform);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            DrawVisionCone();
            DrawLastSample();
        }

        private void DrawVisionCone()
        {
            var observer = eye != null ? eye : transform;
            var origin = observer.position;
            var forward = observer.forward.sqrMagnitude > Mathf.Epsilon
                ? observer.forward.normalized
                : Vector3.forward;
            var right = Vector3.Cross(forward, Vector3.up);
            if (right.sqrMagnitude <= Mathf.Epsilon)
            {
                right = Vector3.Cross(forward, Vector3.right);
            }

            right.Normalize();
            var up = Vector3.Cross(right, forward).normalized;
            var halfAngleRadians = fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad;
            var coneCenter = origin + forward * (Mathf.Cos(halfAngleRadians) * maximumDistance);
            var coneRadius = Mathf.Sin(halfAngleRadians) * maximumDistance;

            Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.75f);
            var previousPoint = coneCenter + right * coneRadius;
            for (var index = 1; index <= ConeSegments; index++)
            {
                var radians = Mathf.PI * 2f * index / ConeSegments;
                var point = coneCenter
                    + (right * Mathf.Cos(radians) + up * Mathf.Sin(radians)) * coneRadius;
                Gizmos.DrawLine(previousPoint, point);
                if (index % 6 == 0)
                {
                    Gizmos.DrawLine(origin, point);
                }

                previousPoint = point;
            }
        }

        private void DrawLastSample()
        {
            if (!hasSampledRay)
            {
                return;
            }

            Gizmos.color = !lastRayWasCandidate
                ? new Color(0.55f, 0.55f, 0.55f, 1f)
                : lastRayBlocked
                    ? new Color(1f, 0.15f, 0.1f, 1f)
                    : new Color(0.2f, 1f, 0.25f, 1f);
            Gizmos.DrawLine(lastRayOrigin, lastRayBlocked ? lastBlockingPoint : lastRayEnd);

            if (lastRayBlocked)
            {
                Gizmos.DrawWireCube(lastBlockingPoint, Vector3.one * 0.18f);
                Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
                Gizmos.DrawLine(lastBlockingPoint, lastRayEnd);
            }

            Gizmos.color = lastTargetDetected
                ? new Color(0.2f, 1f, 0.25f, 1f)
                : new Color(1f, 0.75f, 0.1f, 1f);
            Gizmos.DrawWireSphere(lastRayEnd, 0.18f + lastDetectionProgress * 0.18f);
        }
    }
}
