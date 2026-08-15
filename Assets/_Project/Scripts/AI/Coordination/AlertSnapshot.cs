using UnityEngine;

namespace GiscardPunk77.AI.Coordination
{
    /// <summary>
    /// Immutable information known at the start of a scene alert.
    /// This is a value snapshot, never a live target reference.
    /// </summary>
    public readonly struct AlertSnapshot
    {
        public AlertSnapshot(bool hasInitialObservation, Vector3 initialObservationPoint, float initialObservationTime)
        {
            HasInitialObservation = hasInitialObservation;
            InitialObservationPoint = initialObservationPoint;
            InitialObservationTime = initialObservationTime;
        }

        public bool HasInitialObservation { get; }
        public Vector3 InitialObservationPoint { get; }
        public float InitialObservationTime { get; }

        public static AlertSnapshot None => new AlertSnapshot(false, Vector3.zero, float.NegativeInfinity);
    }
}
