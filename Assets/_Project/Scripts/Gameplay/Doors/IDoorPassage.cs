using System;
using UnityEngine;

namespace GiscardPunk77.Gameplay.Doors
{
    /// <summary>
    /// Common passage API used by players, NPC orchestration and test doubles.
    /// </summary>
    public interface IDoorPassage
    {
        bool CanUse { get; }

        bool IsPassable { get; }

        Transform WaitingPointA { get; }

        Transform WaitingPointB { get; }

        event Action<DoorPassageState> StateChanged;

        event Action<object> ReservationChanged;

        bool RequestOpen();

        bool TryReserve(object owner);

        bool IsReservedBy(object owner);

        void Release(object owner);
    }
}
