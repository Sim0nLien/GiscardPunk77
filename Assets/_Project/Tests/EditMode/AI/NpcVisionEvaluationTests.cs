using GiscardPunk77.AI.Perception;
using NUnit.Framework;
using UnityEngine;

namespace GiscardPunk77.AI.Tests
{
    public sealed class NpcVisionEvaluationTests
    {
        private static readonly NpcVisionParameters Parameters = new NpcVisionParameters(
            12f,
            100f,
            0.6f,
            0.65f,
            1.75f);

        [Test]
        public void StandingTargetInFrontProducesCandidateScore()
        {
            var score = Evaluate(new Vector3(0f, 0f, 6f), false);

            Assert.That(score.IsCandidate, Is.True);
            Assert.That(score.ViewAngleDegrees, Is.EqualTo(0f).Within(0.001f));
            Assert.That(score.VisibilityScore, Is.GreaterThan(0f));
        }

        [Test]
        public void TargetBehindObserverIsOutsideViewCone()
        {
            var score = Evaluate(new Vector3(0f, 0f, -2f), false);

            Assert.That(score.IsInsideDistance, Is.True);
            Assert.That(score.IsInsideView, Is.False);
            Assert.That(score.IsCandidate, Is.False);
        }

        [Test]
        public void CrouchingTargetHasShorterMaximumDistance()
        {
            var target = new Vector3(0f, 0f, 9f);

            Assert.That(Evaluate(target, false).IsInsideDistance, Is.True);
            Assert.That(Evaluate(target, true).IsInsideDistance, Is.False);
        }

        [Test]
        public void StandingTargetReachesDetectionBeforeCrouchingTarget()
        {
            var standingScore = Evaluate(new Vector3(0f, 0f, 4f), false);
            var crouchingScore = Evaluate(new Vector3(0f, 0f, 4f), true);
            var standingExposure = 0f;
            var crouchingExposure = 0f;

            for (var index = 0; index < 9; index++)
            {
                standingExposure = NpcVisionEvaluation.AdvanceExposure(
                    standingExposure, 0.1f, standingScore, false, false, Parameters);
                crouchingExposure = NpcVisionEvaluation.AdvanceExposure(
                    crouchingExposure, 0.1f, crouchingScore, false, true, Parameters);
            }

            var standingProgress = NpcVisionEvaluation.CalculateDetectionProgress(
                standingExposure,
                Parameters.GetRequiredExposure(false));
            var crouchingProgress = NpcVisionEvaluation.CalculateDetectionProgress(
                crouchingExposure,
                Parameters.GetRequiredExposure(true));

            Assert.That(standingProgress, Is.EqualTo(1f));
            Assert.That(crouchingProgress, Is.LessThan(standingProgress));
        }

        [Test]
        public void OcclusionClearsContinuousExposure()
        {
            var score = Evaluate(new Vector3(0f, 0f, 4f), false);
            var exposure = NpcVisionEvaluation.AdvanceExposure(
                0f, 0.2f, score, false, false, Parameters);

            exposure = NpcVisionEvaluation.AdvanceExposure(
                exposure, 0.1f, score, true, false, Parameters);

            Assert.That(exposure, Is.Zero);
        }

        [Test]
        public void ExposureAndProgressAreBounded()
        {
            var score = Evaluate(new Vector3(0f, 0f, 1f), false);
            var exposure = NpcVisionEvaluation.AdvanceExposure(
                99f, 99f, score, false, false, Parameters);

            Assert.That(exposure, Is.EqualTo(Parameters.GetRequiredExposure(false)));
            Assert.That(NpcVisionEvaluation.CalculateDetectionProgress(exposure, 0f), Is.EqualTo(1f));
        }

        [Test]
        public void SamplingPhaseIsDeterministicBoundedAndDistributed()
        {
            var first = NpcVisionEvaluation.CalculateSamplingPhase01(101);
            var second = NpcVisionEvaluation.CalculateSamplingPhase01(202);

            Assert.That(first, Is.InRange(0f, 1f));
            Assert.That(second, Is.InRange(0f, 1f));
            Assert.That(NpcVisionEvaluation.CalculateSamplingPhase01(101), Is.EqualTo(first));
            Assert.That(second, Is.Not.EqualTo(first));
            Assert.That(
                NpcVisionEvaluation.CalculateInitialSampleDelay(101, 0.25f),
                Is.InRange(0f, 0.25f));
        }

        private static NpcVisionScore Evaluate(Vector3 target, bool isCrouching)
        {
            return NpcVisionEvaluation.EvaluateGeometry(
                Vector3.zero,
                Vector3.forward,
                target,
                isCrouching,
                Parameters);
        }
    }
}
