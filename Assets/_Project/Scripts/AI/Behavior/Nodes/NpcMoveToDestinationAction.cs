using System;
using GiscardPunk77.AI.Navigation;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Nodes
{
    /// <summary>Delegates one route to NpcMotor and observes its public status.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "NPC Move To Destination",
        description: "Commands NpcMotor; it never manipulates NavMeshAgent directly.",
        story: "Move [Context] to [Destination]",
        category: "GiscardPunk77/NPC",
        id: "cf61af586ecc4bebab1cd47c6a060d3a")]
    public partial class NpcMoveToDestinationAction : Action
    {
        [SerializeReference] public BlackboardVariable<NpcContext> Context = new();
        [SerializeReference] public BlackboardVariable<Vector3> Destination = new(Vector3.zero);

        [NonSerialized] private NpcMotor activeMotor;
        [NonSerialized] private bool ownsRoute;

        protected override Status OnStart()
        {
            ownsRoute = false;
            activeMotor = null;

            var context = Context?.Value;
            if (context == null)
            {
                return Fail("NpcContext is not assigned.");
            }

            if (!context.TryValidate(NpcContextRequirement.Motor, out var error))
            {
                return Fail(error);
            }

            activeMotor = context.Motor;
            if (!activeMotor.isActiveAndEnabled)
            {
                return Fail("NpcMotor is disabled; movement was not started.");
            }

            if (Destination == null)
            {
                return Fail("Destination is not assigned.");
            }

            if (!activeMotor.TrySetDestination(Destination.Value))
            {
                return Fail($"NpcMotor rejected the destination ({activeMotor.FailureReason}).");
            }

            ownsRoute = true;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (activeMotor == null)
            {
                return Fail("NpcMotor reference was lost while moving.");
            }

            if (!activeMotor.isActiveAndEnabled)
            {
                return Fail("NpcMotor was disabled while moving.");
            }

            return activeMotor.Status switch
            {
                NpcMotorStatus.Moving or NpcMotorStatus.Waiting => Status.Running,
                NpcMotorStatus.Arrived => Status.Success,
                NpcMotorStatus.Failed =>
                    Fail($"NpcMotor failed while moving ({activeMotor.FailureReason})."),
                _ => Fail($"NpcMotor stopped the route unexpectedly ({activeMotor.Status}).")
            };
        }

        protected override void OnEnd()
        {
            if (ownsRoute && activeMotor != null)
            {
                activeMotor.Cancel();
            }

            ownsRoute = false;
            activeMotor = null;
        }

        private Status Fail(string reason)
        {
            LogFailure(reason, true);
            return Status.Failure;
        }
    }
}
