using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Nodes
{
    /// <summary>Copies an explicitly authored destination object's position into the graph blackboard.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "NPC Choose Destination",
        description: "Chooses an explicitly referenced destination without pathfinding or scene searches.",
        story: "Choose [Source] as [Destination]",
        category: "GiscardPunk77/NPC",
        id: "8b7c34d9d1ac4f6da45962cd20b0ee6d")]
    public partial class NpcChooseDestinationAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Source = new();
        [SerializeReference] public BlackboardVariable<Vector3> Destination = new(Vector3.zero);

        protected override Status OnStart()
        {
            if (Source?.Value == null)
            {
                LogFailure("Destination Source is not assigned.", true);
                return Status.Failure;
            }

            if (Destination == null)
            {
                LogFailure("Destination output is not assigned.", true);
                return Status.Failure;
            }

            Destination.Value = Source.Value.transform.position;
            return Status.Success;
        }
    }
}
