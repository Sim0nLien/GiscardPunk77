using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    /// <summary>Validated tuning values used by the pure vision calculation.</summary>
    public readonly struct NpcVisionParameters
    {
        public NpcVisionParameters(
            float maximumDistance,
            float fieldOfViewDegrees,
            float standingExposureSeconds,
            float crouchingDistanceMultiplier,
            float crouchingExposureMultiplier)
        {
            MaximumDistance = Mathf.Max(0.1f, maximumDistance);
            FieldOfViewDegrees = Mathf.Clamp(fieldOfViewDegrees, 1f, 179f);
            StandingExposureSeconds = Mathf.Max(0.01f, standingExposureSeconds);
            CrouchingDistanceMultiplier = Mathf.Clamp(crouchingDistanceMultiplier, 0.05f, 1f);
            CrouchingExposureMultiplier = Mathf.Max(1f, crouchingExposureMultiplier);
        }

        public float MaximumDistance { get; }
        public float FieldOfViewDegrees { get; }
        public float StandingExposureSeconds { get; }
        public float CrouchingDistanceMultiplier { get; }
        public float CrouchingExposureMultiplier { get; }

        public float GetMaximumDistance(bool isCrouching)
        {
            return MaximumDistance * (isCrouching ? CrouchingDistanceMultiplier : 1f);
        }

        public float GetRequiredExposure(bool isCrouching)
        {
            return StandingExposureSeconds * (isCrouching ? CrouchingExposureMultiplier : 1f);
        }

        public float GetPostureCoefficient(bool isCrouching)
        {
            return isCrouching ? CrouchingDistanceMultiplier : 1f;
        }
    }
}
