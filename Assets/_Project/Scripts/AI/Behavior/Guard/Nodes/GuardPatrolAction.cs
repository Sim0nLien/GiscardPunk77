using System;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.AI.Perception;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Guard.Nodes
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Guard Patrol",
        description: "Moves to the next explicitly authored patrol point through NpcMotor.",
        story: "Patrol guard [Context]",
        category: "GiscardPunk77/Guard",
        id: "0a8499b64398471ab97c3b321cf813ab")]
    public partial class GuardPatrolAction : Action
    {
        [SerializeReference] public BlackboardVariable<GuardContext> Context = new();

        protected override Status OnStart()
        {
            var context = Context?.Value;
            if (context == null)
            {
                return Fail("GuardContext is not assigned.");
            }

            if (!context.TryValidate(out var error))
            {
                return Fail(error);
            }

            context.EnterState(GuardState.Patrol, $"Patrol point {context.PatrolPointIndex + 1}");
            if (!context.TryGetCurrentPatrolPoint(out var point))
            {
                context.RequestState(GuardState.Idle);
                return Status.Success;
            }

            return context.Npc.Motor.TrySetDestination(point)
                ? Status.Running
                : Fail($"NpcMotor rejected patrol point ({context.Npc.Motor.FailureReason}).");
        }

        protected override Status OnUpdate()
        {
            var context = Context?.Value;
            if (context == null)
            {
                return Fail("GuardContext reference was lost.");
            }

            if (context.Awareness.State != NpcAwarenessState.Unaware && context.TryGetLastKnownPosition(out _))
            {
                context.Npc.Motor.Cancel();
                context.RequestState(GuardState.Suspicious);
                return Status.Success;
            }

            return context.Npc.Motor.Status switch
            {
                NpcMotorStatus.Moving or NpcMotorStatus.Waiting => Status.Running,
                NpcMotorStatus.Arrived => CompletePatrolLeg(context),
                NpcMotorStatus.Failed => Fail($"NpcMotor failed during patrol ({context.Npc.Motor.FailureReason})."),
                _ => Fail($"NpcMotor stopped patrol unexpectedly ({context.Npc.Motor.Status}).")
            };
        }

        protected override void OnEnd()
        {
            var context = Context?.Value;
            if (context != null && context.Npc != null && context.Npc.Motor != null &&
                context.CurrentState != GuardState.GlobalAlerted)
            {
                context.Npc.Motor.Cancel();
            }
        }

        private static Status CompletePatrolLeg(GuardContext context)
        {
            context.Npc.Motor.Cancel();
            context.AdvancePatrolPoint();
            context.RequestState(GuardState.Idle);
            return Status.Success;
        }

        private Status Fail(string reason)
        {
            LogFailure(reason, true);
            return Status.Failure;
        }
    }
}
