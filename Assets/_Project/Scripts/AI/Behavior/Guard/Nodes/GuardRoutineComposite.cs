using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Guard.Nodes
{
    /// <summary>Routes between four thin guard actions and fails immediately on global alert.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Guard Routine",
        description: "Runs Idle, Patrol, Suspicious or Investigation and aborts on the scene alert.",
        story: "Run non-combat routine for [Context]",
        category: "GiscardPunk77/Guard",
        id: "474b9b55f41b4c73a60f9a167e5e9411")]
    public partial class GuardRoutineComposite : Composite
    {
        [SerializeReference] public BlackboardVariable<GuardContext> Context = new();

        [NonSerialized] private Node activeChild;

        protected override Status OnStart()
        {
            activeChild = null;
            var context = Context?.Value;
            if (context == null)
            {
                return Fail("GuardContext is not assigned.");
            }

            if (!context.TryValidate(out var error))
            {
                return Fail(error);
            }

            if (Children.Count < 4)
            {
                return Fail("Guard Routine requires four children in this order: Idle, Patrol, Suspicious, Investigate.");
            }

            if (context.IsGloballyAlerted)
            {
                context.EnterState(GuardState.GlobalAlerted, "Global alert was already active");
                return Status.Failure;
            }

            context.RequestState(GuardState.Idle);
            return StartRequestedChild(context);
        }

        protected override Status OnUpdate()
        {
            var context = Context?.Value;
            if (context == null)
            {
                return Fail("GuardContext reference was lost.");
            }

            if (context.IsGloballyAlerted)
            {
                if (activeChild is { CurrentStatus: Status.Running or Status.Waiting })
                {
                    EndNode(activeChild);
                }

                context.EnterState(GuardState.GlobalAlerted, "Global alert interrupted routine");
                return Status.Failure;
            }

            if (activeChild == null)
            {
                return StartRequestedChild(context);
            }

            return activeChild.CurrentStatus switch
            {
                Status.Running or Status.Waiting => Status.Waiting,
                Status.Success => StartRequestedChild(context),
                Status.Failure => Status.Failure,
                _ => Status.Running
            };
        }

        private Status StartRequestedChild(GuardContext context)
        {
            var index = context.RequestedState switch
            {
                GuardState.Patrol => 1,
                GuardState.Suspicious => 2,
                GuardState.InvestigateLastKnownPosition => 3,
                _ => 0
            };

            activeChild = Children[index];
            var status = StartNode(activeChild);
            return status switch
            {
                Status.Failure => Status.Failure,
                Status.Running or Status.Waiting => Status.Waiting,
                _ => Status.Running
            };
        }

        private Status Fail(string reason)
        {
            LogFailure(reason, true);
            return Status.Failure;
        }
    }
}
