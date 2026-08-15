using System;

namespace GiscardPunk77.Core
{
    [Serializable]
    public readonly struct ActorIdentity : IEquatable<ActorIdentity>
    {
        public ActorIdentity(ActorKind kind, TeamId team)
        {
            Kind = kind;
            Team = team;
        }

        public ActorKind Kind { get; }

        public TeamId Team { get; }

        public bool Equals(ActorIdentity other)
        {
            return Kind == other.Kind && Team == other.Team;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ Team.GetHashCode();
            }
        }

        public static bool operator ==(ActorIdentity left, ActorIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorIdentity left, ActorIdentity right)
        {
            return !left.Equals(right);
        }
    }
}
