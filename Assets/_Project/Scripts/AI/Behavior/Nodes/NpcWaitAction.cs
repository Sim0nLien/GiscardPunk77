using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace GiscardPunk77.AI.Behavior.Nodes
{
    /// <summary>Waits using scaled frame time so pause and graph restart remain predictable.</summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "NPC Wait",
        description: "Waits for a bounded duration without owning gameplay state.",
        story: "Wait [Seconds] seconds",
        category: "GiscardPunk77/NPC",
        id: "e8cad39a4dc34a32be302f764781dced")]
    public partial class NpcWaitAction : Action
    {
        [SerializeReference] public BlackboardVariable<float> Seconds = new(0.5f);

        [NonSerialized] private float elapsedSeconds;
        [NonSerialized] private float durationSeconds;

        protected override Status OnStart()
        {
            elapsedSeconds = 0f;
            durationSeconds = Mathf.Max(0f, Seconds?.Value ?? 0f);
            return durationSeconds <= 0f ? Status.Success : Status.Running;
        }

        protected override Status OnUpdate()
        {
            elapsedSeconds += Time.deltaTime;
            return elapsedSeconds >= durationSeconds ? Status.Success : Status.Running;
        }
    }
}
