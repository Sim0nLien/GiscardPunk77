using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Nodes
{
    /// <summary>Reads one explicit component state. Place it in Conditional Guard or Abort nodes.</summary>
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "NPC State",
        description: "Checks a state already owned by the NPC components.",
        story: "[Context] matches [Query]",
        category: "GiscardPunk77/NPC",
        id: "4f519c0db29148fbbc1a1b2d3cc14e0f")]
    public partial class NpcStateCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<NpcContext> Context = new();
        [SerializeReference] public BlackboardVariable<NpcStateQuery> Query =
            new(NpcStateQuery.DeadOrGloballyAlerted);

        [NonSerialized] private bool hasLoggedError;

        public override void OnStart()
        {
            hasLoggedError = false;
        }

        public override bool IsTrue()
        {
            if (Query == null)
            {
                LogErrorOnce("NpcStateCondition: Query is not assigned.");
                return false;
            }

            if (NpcStateReader.TryMatches(Context?.Value, Query.Value, out var matches, out var error))
            {
                return matches;
            }

            LogErrorOnce($"NpcStateCondition: {error}");

            return false;
        }

        private void LogErrorOnce(string error)
        {
            if (hasLoggedError)
            {
                return;
            }

            Debug.LogError(error, GameObject);
            hasLoggedError = true;
        }
    }
}
