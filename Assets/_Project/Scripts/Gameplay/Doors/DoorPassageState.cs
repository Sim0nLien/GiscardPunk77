using System;

namespace GiscardPunk77.Gameplay.Doors
{
    /// <summary>Immutable observable snapshot of a door passage.</summary>
    public readonly struct DoorPassageState : IEquatable<DoorPassageState>
    {
        public DoorPassageState(
            bool canUse,
            bool openRequested,
            bool isPassable,
            object reservationOwner,
            int queueCount)
        {
            CanUse = canUse;
            OpenRequested = openRequested;
            IsPassable = isPassable;
            ReservationOwner = reservationOwner;
            QueueCount = queueCount;
        }

        public bool CanUse { get; }

        public bool OpenRequested { get; }

        public bool IsPassable { get; }

        public object ReservationOwner { get; }

        public int QueueCount { get; }

        public bool Equals(DoorPassageState other)
        {
            return CanUse == other.CanUse
                && OpenRequested == other.OpenRequested
                && IsPassable == other.IsPassable
                && ReferenceEquals(ReservationOwner, other.ReservationOwner)
                && QueueCount == other.QueueCount;
        }

        public override bool Equals(object obj)
        {
            return obj is DoorPassageState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = CanUse ? 1 : 0;
                hash = (hash * 397) ^ (OpenRequested ? 1 : 0);
                hash = (hash * 397) ^ (IsPassable ? 1 : 0);
                hash = (hash * 397) ^ (ReservationOwner != null ? ReservationOwner.GetHashCode() : 0);
                hash = (hash * 397) ^ QueueCount;
                return hash;
            }
        }
    }
}
