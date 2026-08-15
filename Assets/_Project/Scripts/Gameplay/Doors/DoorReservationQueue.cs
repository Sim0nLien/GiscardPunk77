using System;
using System.Collections.Generic;

namespace GiscardPunk77.Gameplay.Doors
{
    /// <summary>
    /// Small FIFO queue with one active owner. Callers refresh their request by calling TryReserve.
    /// </summary>
    public sealed class DoorReservationQueue
    {
        private const int InitialCapacity = 8;
        private const float MinimumLifetime = 0.01f;

        private readonly List<Entry> entries = new List<Entry>(InitialCapacity);

        public int Count => entries.Count;

        public object ActiveOwner => entries.Count > 0 ? entries[0].Owner : null;

        public int Version { get; private set; }

        public bool TryReserve(object owner, float currentTime, float lifetime)
        {
            if (owner == null || float.IsNaN(currentTime) || float.IsInfinity(currentTime))
            {
                return false;
            }

            RemoveExpired(currentTime);
            var expiresAt = currentTime + Math.Max(MinimumLifetime, lifetime);
            for (var index = 0; index < entries.Count; index++)
            {
                if (!ReferenceEquals(entries[index].Owner, owner))
                {
                    continue;
                }

                entries[index] = new Entry(owner, expiresAt);
                return index == 0;
            }

            entries.Add(new Entry(owner, expiresAt));
            Version++;
            return entries.Count == 1;
        }

        public bool IsReservedBy(object owner)
        {
            return owner != null && entries.Count > 0 && ReferenceEquals(entries[0].Owner, owner);
        }

        public bool IsQueued(object owner)
        {
            if (owner == null)
            {
                return false;
            }

            for (var index = 0; index < entries.Count; index++)
            {
                if (ReferenceEquals(entries[index].Owner, owner))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Release(object owner)
        {
            if (owner == null)
            {
                return false;
            }

            for (var index = 0; index < entries.Count; index++)
            {
                if (!ReferenceEquals(entries[index].Owner, owner))
                {
                    continue;
                }

                entries.RemoveAt(index);
                Version++;
                return true;
            }

            return false;
        }

        public int RemoveExpired(float currentTime)
        {
            if (float.IsNaN(currentTime) || float.IsInfinity(currentTime))
            {
                return 0;
            }

            var removed = 0;
            for (var index = entries.Count - 1; index >= 0; index--)
            {
                if (entries[index].ExpiresAt > currentTime)
                {
                    continue;
                }

                entries.RemoveAt(index);
                removed++;
            }

            if (removed > 0)
            {
                Version++;
            }

            return removed;
        }

        public void Clear()
        {
            if (entries.Count == 0)
            {
                return;
            }

            entries.Clear();
            Version++;
        }

        private readonly struct Entry
        {
            public Entry(object owner, float expiresAt)
            {
                Owner = owner;
                ExpiresAt = expiresAt;
            }

            public object Owner { get; }

            public float ExpiresAt { get; }
        }
    }
}
