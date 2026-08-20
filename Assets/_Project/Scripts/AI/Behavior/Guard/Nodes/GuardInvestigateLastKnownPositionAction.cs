using System;
using GiscardPunk77.AI.Navigation;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Guard.Nodes
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Guard Investigate Last Known Position",
        description: "Visits one remembered point, waits briefly, then returns to the captured post.",
        story: "Investigate last known position for [Context]",
        category: "GiscardPunk77/Guard",
        id: "712f19f0babb42df9b24bfe2f7649f40")]
    public partial class GuardInvestigateLastKnownPositionAction : Action
    {
        private enum Stage
        {
            MoveToLastKnown,
            Observe,
            ReturnToPost
        }

        [SerializeReference] public BlackboardVariable<GuardContext> Context = new();

        [NonSerialized] private Stage stage;
        [NonSerialized] private float stageDeadline;

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

            if (!context.TryGetLastKnownPosition(out var lastKnownPosition))
            {
                context.RequestState(GuardState.Idle);
                return Status.Success;
            }

            context.EnterState(GuardState.InvestigateLastKnownPosition, "Orient phase completed");
            stage = Stage.MoveToLastKnown;
            stageDeadline = Time.time + context.Config.InvestigationTimeoutSeconds;
            return context.Npc.Motor.TrySetDestination(lastKnownPosition)
                ? Status.Running
                : BeginReturnToPost(context);
        }

        protected override Status OnUpdate()
        {
            var context = Context?.Value;
            if (context == null)
            {
                return Fail("GuardContext reference was lost.");
            }

            switch (stage)
            {
                case Stage.MoveToLastKnown:
                    if (context.Npc.Motor.Status == NpcMotorStatus.Arrived)
                    {
                        context.Npc.Motor.Cancel();
                        stage = Stage.Observe;
                        stageDeadline = Time.time + context.Config.InvestigationWaitSeconds;
                    }
                    else if (context.Npc.Motor.Status == NpcMotorStatus.Failed || Time.time >= stageDeadline)
                    {
                        return BeginReturnToPost(context);
                    }

                    return Status.Running;

                case Stage.Observe:
                    if (Time.time >= stageDeadline)
                    {
                        return BeginReturnToPost(context);
                    }

                    return Status.Running;

                default:
                    if (context.Npc.Motor.Status == NpcMotorStatus.Arrived)
                    {
                        context.Npc.Motor.Cancel();
                        context.RequestState(GuardState.Idle);
                        return Status.Success;
                    }

                    if (context.Npc.Motor.Status == NpcMotorStatus.Failed || Time.time >= stageDeadline)
                    {
                        return Fail($"Guard could not return to post ({context.Npc.Motor.FailureReason}).");
                    }

                    return Status.Running;
            }
        }

        protected override void OnEnd()
        {
            var context = Context?.Value;
            if (context != null && context.Npc != null && context.Npc.Motor != null)
            {
                context.Npc.Motor.Cancel();
            }
        }

        private Status BeginReturnToPost(GuardContext context)
        {
            context.Npc.Motor.Cancel();
            stage = Stage.ReturnToPost;
            stageDeadline = Time.time + context.Config.ReturnToPostTimeoutSeconds;
            return context.Npc.Motor.TrySetDestination(context.PostPosition)
                ? Status.Running
                : Fail($"NpcMotor rejected the guard post ({context.Npc.Motor.FailureReason}).");
        }

        private Status Fail(string reason)
        {
            LogFailure(reason, true);
            return Status.Failure;
        }
    }
}
