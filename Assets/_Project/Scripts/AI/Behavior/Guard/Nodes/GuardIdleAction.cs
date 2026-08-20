using System;
using GiscardPunk77.AI.Perception;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Guard.Nodes
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Guard Idle",
        description: "Waits at the post, then requests patrol; suspicion preempts the wait.",
        story: "Idle guard [Context]",
        category: "GiscardPunk77/Guard",
        id: "7d85969a5a704a419303f90c3fbe7391")]
    public partial class GuardIdleAction : Action
    {
        [SerializeReference] public BlackboardVariable<GuardContext> Context = new();
        [NonSerialized] private float elapsed;

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

            context.Npc.Motor.Cancel();
            context.EnterState(GuardState.Idle, "Returned to post or completed patrol leg");
            elapsed = 0f;
            return Status.Running;
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
                context.RequestState(GuardState.Suspicious);
                return Status.Success;
            }

            elapsed += Time.deltaTime;
            if (elapsed < context.Config.IdleSeconds)
            {
                return Status.Running;
            }

            context.RequestState(context.PatrolRoute.Count > 0 ? GuardState.Patrol : GuardState.Idle);
            return Status.Success;
        }

        private Status Fail(string reason)
        {
            LogFailure(reason, true);
            return Status.Failure;
        }
    }
}
