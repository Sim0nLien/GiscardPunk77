using UnityEngine.AI;

namespace GiscardPunk77.AI.Navigation
{
    /// <summary>
    /// Initial P04 measurements shared by the authoring tool, the probe and tests.
    /// These are sandbox defaults, not a final NPC balance asset.
    /// </summary>
    public static class NpcSandboxTuning
    {
        public const int HumanoidAgentTypeId = 0;

        public const float AgentRadius = 0.5f;
        public const float AgentHeight = 2f;
        public const float AgentSpeed = 3.5f;
        public const float AgentAngularSpeed = 360f;
        public const float AgentAcceleration = 12f;
        public const float AgentStoppingDistance = 0.08f;
        public const int AgentAvoidancePriority = 50;
        public const ObstacleAvoidanceType AgentAvoidance = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        public const float NarrowCorridorWidth = 1.5f;
        public const float PassingBayWidth = 4.6f;
        public const float DoorOpeningWidth = 1.5f;
        public const float ThresholdLinkWidth = 1.2f;
        public const float ThresholdGapLength = 0.3f;

        public const float VoxelSize = 0.125f;
        public const int TileSize = 128;
        public const float MinimumRegionArea = 0.5f;
    }
}
