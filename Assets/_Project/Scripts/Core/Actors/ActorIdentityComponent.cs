using UnityEngine;

namespace GiscardPunk77.Core
{
    /// <summary>Unity authoring component for the immutable ActorIdentity value.</summary>
    [DisallowMultipleComponent]
    public sealed class ActorIdentityComponent : MonoBehaviour
    {
        [SerializeField] private ActorKind kind = ActorKind.Unknown;
        [SerializeField] private int teamId;

        public ActorIdentity Identity => new ActorIdentity(kind, new TeamId(teamId));
        public ActorKind Kind => kind;
        public TeamId Team => new TeamId(teamId);

        public void Configure(ActorKind actorKind, TeamId team)
        {
            kind = actorKind;
            teamId = team.Value;
        }
    }
}
