using System;

namespace GiscardPunk77.Core
{
    [Serializable]
    public readonly struct TeamId : IEquatable<TeamId>
    {
        public static readonly TeamId Neutral = new TeamId(0);

        public TeamId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool Equals(TeamId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is TeamId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(TeamId left, TeamId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TeamId left, TeamId right)
        {
            return !left.Equals(right);
        }
    }
}
