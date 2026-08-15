using System;
using UnityEngine;

namespace GiscardPunk77.AI.Coordination
{
    /// <summary>
    /// A scene-owned alert channel. Place and reference one instance explicitly in a scene.
    /// It deliberately contains no target Transform and performs no automatic position update.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AlertService : MonoBehaviour
    {
        [Header("Runtime diagnostic (read only)")]
        [SerializeField] private AlertLevel level;
        [SerializeField] private bool hasInitialObservation;
        [SerializeField] private Vector3 initialObservationPoint;
        [SerializeField] private float initialObservationTime = float.NegativeInfinity;

        public AlertLevel Level => level;
        public bool IsAlerted => level == AlertLevel.Alerted;
        public AlertSnapshot Snapshot => new AlertSnapshot(
            hasInitialObservation,
            initialObservationPoint,
            initialObservationTime);

        /// <summary>Raised only when the shared level actually changes.</summary>
        public event Action<AlertLevel, AlertSnapshot> LevelChanged;

        /// <returns>True only for the first transition from Calm to Alerted.</returns>
        public bool TryRaiseAlert(AlertSnapshot initialSnapshot)
        {
            if (level == AlertLevel.Alerted)
            {
                return false;
            }

            level = AlertLevel.Alerted;
            hasInitialObservation = initialSnapshot.HasInitialObservation;
            initialObservationPoint = initialSnapshot.InitialObservationPoint;
            initialObservationTime = initialSnapshot.InitialObservationTime;
            LevelChanged?.Invoke(level, Snapshot);
            return true;
        }

        /// <summary>Returns the scene to Calm and clears the frozen initial observation.</summary>
        [ContextMenu("P09/Reset Alert")]
        public bool ResetAlert()
        {
            if (level == AlertLevel.Calm)
            {
                return false;
            }

            level = AlertLevel.Calm;
            hasInitialObservation = false;
            initialObservationPoint = Vector3.zero;
            initialObservationTime = float.NegativeInfinity;
            LevelChanged?.Invoke(level, Snapshot);
            return true;
        }
    }
}
