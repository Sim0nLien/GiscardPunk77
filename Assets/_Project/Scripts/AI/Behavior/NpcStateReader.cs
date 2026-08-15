using GiscardPunk77.AI.Navigation;
using GiscardPunk77.AI.Perception;

namespace GiscardPunk77.AI.Behavior
{
    /// <summary>Centralizes state reads so Behavior conditions contain no gameplay logic.</summary>
    public static class NpcStateReader
    {
        public static bool TryMatches(
            NpcContext context,
            NpcStateQuery query,
            out bool matches,
            out string error)
        {
            matches = false;
            if (context == null)
            {
                error = "NpcContext is not assigned.";
                return false;
            }

            var requirements = RequirementsFor(query);
            if (!context.TryValidate(requirements, out error))
            {
                return false;
            }

            matches = query switch
            {
                NpcStateQuery.Alive => !context.Health.IsDead,
                NpcStateQuery.Dead => context.Health.IsDead,
                NpcStateQuery.AwarenessUnaware => context.Awareness.State == NpcAwarenessState.Unaware,
                NpcStateQuery.AwarenessSuspicious => context.Awareness.State == NpcAwarenessState.Suspicious,
                NpcStateQuery.AwarenessAlerted => context.Awareness.State == NpcAwarenessState.Alerted,
                NpcStateQuery.GlobalCalm => !context.AlertService.IsAlerted,
                NpcStateQuery.GlobalAlerted => context.AlertService.IsAlerted,
                NpcStateQuery.MotorIdle => context.Motor.Status == NpcMotorStatus.Idle,
                NpcStateQuery.MotorMoving => context.Motor.Status == NpcMotorStatus.Moving,
                NpcStateQuery.MotorWaiting => context.Motor.Status == NpcMotorStatus.Waiting,
                NpcStateQuery.MotorArrived => context.Motor.Status == NpcMotorStatus.Arrived,
                NpcStateQuery.MotorFailed => context.Motor.Status == NpcMotorStatus.Failed,
                NpcStateQuery.DeadOrGloballyAlerted => context.Health.IsDead || context.AlertService.IsAlerted,
                _ => false
            };
            return true;
        }

        private static NpcContextRequirement RequirementsFor(NpcStateQuery query)
        {
            return query switch
            {
                NpcStateQuery.Alive or NpcStateQuery.Dead => NpcContextRequirement.Health,
                NpcStateQuery.AwarenessUnaware or
                NpcStateQuery.AwarenessSuspicious or
                NpcStateQuery.AwarenessAlerted => NpcContextRequirement.Awareness,
                NpcStateQuery.GlobalCalm or NpcStateQuery.GlobalAlerted => NpcContextRequirement.AlertService,
                NpcStateQuery.MotorIdle or
                NpcStateQuery.MotorMoving or
                NpcStateQuery.MotorWaiting or
                NpcStateQuery.MotorArrived or
                NpcStateQuery.MotorFailed => NpcContextRequirement.Motor,
                NpcStateQuery.DeadOrGloballyAlerted =>
                    NpcContextRequirement.Health | NpcContextRequirement.AlertService,
                _ => NpcContextRequirement.None
            };
        }
    }
}
