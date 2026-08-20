using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Guard.Nodes
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Guard Suspicious",
        description: "Turns toward the remembered observation before requesting investigation.",
        story: "Orient suspicious guard [Context]",
        category: "GiscardPunk77/Guard",
        id: "608a4e85657d426f99020ce0358e0930")]
    public partial class GuardSuspiciousAction : Action
    {
        [SerializeReference] public BlackboardVariable<GuardContext> Context = new();

        [NonSerialized] private float elapsed;
        [NonSerialized] private Vector3 lastKnownPosition;

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
            if (!context.TryGetLastKnownPosition(out lastKnownPosition))
            {
                context.RequestState(GuardState.Idle);
                return Status.Success;
            }

            context.EnterState(GuardState.Suspicious, "Suspicion interrupted routine");
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

            if (context.TryGetLastKnownPosition(out var refreshedPosition))
            {
                lastKnownPosition = refreshedPosition;
            }

            context.Npc.Motor.TryRotateTowards(lastKnownPosition);
            elapsed += Time.deltaTime;
            if (elapsed < context.Config.SuspiciousOrientationSeconds)
            {
                return Status.Running;
            }

            context.RequestState(GuardState.InvestigateLastKnownPosition);
            return Status.Success;
        }

        private Status Fail(string reason)
        {
            LogFailure(reason, true);
            return Status.Failure;
        }
    }
}
