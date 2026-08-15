using UnityEngine;

namespace GiscardPunk77.AI.Perception
{
    /// <summary>Deterministic vision functions kept separate from Physics and MonoBehaviour.</summary>
    public static class NpcVisionEvaluation
    {
        private const float MinimumVisibleGain = 0.25f;

        public static NpcVisionScore EvaluateGeometry(
            Vector3 observerPosition,
            Vector3 observerForward,
            Vector3 targetPoint,
            bool isCrouching,
            NpcVisionParameters parameters)
        {
            var toTarget = targetPoint - observerPosition;
            var distance = toTarget.magnitude;
            var forward = observerForward.sqrMagnitude > Mathf.Epsilon
                ? observerForward.normalized
                : Vector3.forward;
            var direction = distance > Mathf.Epsilon ? toTarget / distance : forward;
            var angle = Vector3.Angle(forward, direction);
            var maximumDistance = parameters.GetMaximumDistance(isCrouching);
            var halfFieldOfView = parameters.FieldOfViewDegrees * 0.5f;
            var isInsideDistance = distance <= maximumDistance;
            var isInsideView = angle <= halfFieldOfView;
            var distanceScore = isInsideDistance
                ? Mathf.Clamp01(1f - distance / maximumDistance)
                : 0f;
            var angleScore = isInsideView
                ? Mathf.Clamp01(1f - angle / halfFieldOfView)
                : 0f;

            return new NpcVisionScore(
                distance,
                angle,
                distanceScore,
                angleScore,
                parameters.GetPostureCoefficient(isCrouching),
                isInsideDistance,
                isInsideView);
        }

        public static float AdvanceExposure(
            float currentExposure,
            float elapsedSeconds,
            NpcVisionScore score,
            bool isOccluded,
            bool isCrouching,
            NpcVisionParameters parameters)
        {
            if (!score.IsCandidate || isOccluded)
            {
                return 0f;
            }

            var geometryScore = score.DistanceScore * score.AngleScore;
            var geometryGain = Mathf.Lerp(MinimumVisibleGain, 1f, geometryScore);
            var gain = geometryGain * score.PostureCoefficient;
            var requiredExposure = parameters.GetRequiredExposure(isCrouching);
            return Mathf.Clamp(
                Mathf.Max(0f, currentExposure) + Mathf.Max(0f, elapsedSeconds) * gain,
                0f,
                requiredExposure);
        }

        public static float CalculateDetectionProgress(float exposureSeconds, float requiredExposureSeconds)
        {
            return Mathf.Clamp01(exposureSeconds / Mathf.Max(0.01f, requiredExposureSeconds));
        }

        public static float CalculateSamplingPhase01(int stableId)
        {
            unchecked
            {
                var value = (uint)stableId;
                value ^= value >> 16;
                value *= 0x7feb352d;
                value ^= value >> 15;
                value *= 0x846ca68b;
                value ^= value >> 16;
                return (value & 0x00ffffff) / 16777216f;
            }
        }

        public static float CalculateInitialSampleDelay(int stableId, float sampleInterval)
        {
            return Mathf.Max(0f, sampleInterval) * CalculateSamplingPhase01(stableId);
        }
    }
}
