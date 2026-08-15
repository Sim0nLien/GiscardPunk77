namespace GiscardPunk77.AI.Perception
{
    /// <summary>Pure geometric result, before the physics occlusion test.</summary>
    public readonly struct NpcVisionScore
    {
        public NpcVisionScore(
            float distance,
            float viewAngleDegrees,
            float distanceScore,
            float angleScore,
            float postureCoefficient,
            bool isInsideDistance,
            bool isInsideView)
        {
            Distance = distance;
            ViewAngleDegrees = viewAngleDegrees;
            DistanceScore = distanceScore;
            AngleScore = angleScore;
            PostureCoefficient = postureCoefficient;
            IsInsideDistance = isInsideDistance;
            IsInsideView = isInsideView;
        }

        public float Distance { get; }
        public float ViewAngleDegrees { get; }
        public float DistanceScore { get; }
        public float AngleScore { get; }
        public float PostureCoefficient { get; }
        public float VisibilityScore => DistanceScore * AngleScore * PostureCoefficient;
        public bool IsInsideDistance { get; }
        public bool IsInsideView { get; }
        public bool IsCandidate => IsInsideDistance && IsInsideView;
    }
}
