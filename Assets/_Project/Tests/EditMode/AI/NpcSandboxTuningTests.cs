using GiscardPunk77.AI.Navigation;
using NUnit.Framework;
using UnityEngine.AI;

namespace GiscardPunk77.AI.Tests
{
    public sealed class NpcSandboxTuningTests
    {
        [Test]
        public void AgentDefaultsMatchTheProjectHumanoidBakeProfile()
        {
            Assert.That(NpcSandboxTuning.HumanoidAgentTypeId, Is.EqualTo(0));
            Assert.That(NpcSandboxTuning.AgentRadius, Is.EqualTo(0.5f));
            Assert.That(NpcSandboxTuning.AgentHeight, Is.EqualTo(2f));
        }

        [Test]
        public void NarrowCorridorFitsExactlyOneAgentWithClearance()
        {
            var agentDiameter = NpcSandboxTuning.AgentRadius * 2f;

            Assert.That(NpcSandboxTuning.NarrowCorridorWidth, Is.GreaterThan(agentDiameter));
            Assert.That(NpcSandboxTuning.NarrowCorridorWidth, Is.LessThan(agentDiameter * 2f));
        }

        [Test]
        public void PassingBayCanFitTwoAgentsSideBySide()
        {
            var minimumTwoAgentWidth = NpcSandboxTuning.AgentRadius * 4f;

            Assert.That(NpcSandboxTuning.PassingBayWidth, Is.GreaterThan(minimumTwoAgentWidth));
        }

        [Test]
        public void ThresholdLinkAndAvoidanceHaveSafeInitialValues()
        {
            Assert.That(
                NpcSandboxTuning.ThresholdLinkWidth,
                Is.GreaterThan(NpcSandboxTuning.AgentRadius * 2f));
            Assert.That(NpcSandboxTuning.AgentAvoidance, Is.EqualTo(ObstacleAvoidanceType.HighQualityObstacleAvoidance));
            Assert.That(NpcSandboxTuning.AgentAvoidancePriority, Is.InRange(0, 99));
        }
    }
}
