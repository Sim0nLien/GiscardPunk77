using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    /// <summary>An immutable snapshot. It reports vision but never changes guard state.</summary>
    public readonly struct NpcVisionObservation
    {
        public NpcVisionObservation(
            bool hasTarget,
            Vector3 observerPoint,
            Vector3 targetPoint,
            float sampleTime,
            bool isCrouching,
            NpcVisionScore score,
            bool isOccluded,
            float exposureSeconds,
            float requiredExposureSeconds)
        {
            HasTarget = hasTarget;
            ObserverPoint = observerPoint;
            TargetPoint = targetPoint;
            SampleTime = sampleTime;
            IsCrouching = isCrouching;
            Distance = score.Distance;
            ViewAngleDegrees = score.ViewAngleDegrees;
            DistanceScore = score.DistanceScore;
            AngleScore = score.AngleScore;
            PostureCoefficient = score.PostureCoefficient;
            VisibilityScore = score.VisibilityScore;
            IsInsideDistance = score.IsInsideDistance;
            IsInsideView = score.IsInsideView;
            IsOccluded = isOccluded;
            ExposureSeconds = exposureSeconds;
            RequiredExposureSeconds = requiredExposureSeconds;
            DetectionProgress = NpcVisionEvaluation.CalculateDetectionProgress(
                exposureSeconds,
                requiredExposureSeconds);
        }

        public bool HasTarget { get; }
        public Vector3 ObserverPoint { get; }
        public Vector3 TargetPoint { get; }
        public float SampleTime { get; }
        public bool IsCrouching { get; }
        public float Distance { get; }
        public float ViewAngleDegrees { get; }
        public float DistanceScore { get; }
        public float AngleScore { get; }
        public float PostureCoefficient { get; }
        public float VisibilityScore { get; }
        public bool IsInsideDistance { get; }
        public bool IsInsideView { get; }
        public bool IsOccluded { get; }
        public bool HasLineOfSight => HasTarget && IsInsideDistance && IsInsideView && !IsOccluded;
        public float ExposureSeconds { get; }
        public float RequiredExposureSeconds { get; }
        public float DetectionProgress { get; }
        public bool IsDetected => HasLineOfSight && DetectionProgress >= 1f;
    }
}
