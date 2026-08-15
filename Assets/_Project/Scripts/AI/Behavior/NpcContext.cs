using System.Collections.Generic;
using GiscardPunk77.AI.Coordination;
using GiscardPunk77.AI.Navigation;
using GiscardPunk77.AI.Perception;
using GiscardPunk77.Core;
using GiscardPunk77.Gameplay;
using UnityEngine;

namespace GiscardPunk77.AI.Behavior
{
    /// <summary>
    /// Explicit dependency bundle consumed by Behavior nodes.
    /// It stores references and diagnostics, but owns no sensing, movement or damage logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcContext : MonoBehaviour
    {
        [Header("Explicit references")]
        [SerializeField] private ActorIdentityComponent identity;
        [SerializeField] private Health health;
        [SerializeField] private NpcMotor motor;
        [SerializeField] private NpcVisionSensor vision;
        [SerializeField] private NpcAwareness awareness;
        [SerializeField] private AlertService alertService;

        [Header("Runtime diagnostic (read only)")]
        [SerializeField] private string lastValidationError;

        public ActorIdentityComponent Identity => identity;
        public Health Health => health;
        public NpcMotor Motor => motor;
        public NpcVisionSensor Vision => vision;
        public NpcAwareness Awareness => awareness;
        public AlertService AlertService => alertService;
        public string LastValidationError => lastValidationError;
        public bool IsDead => health != null && health.IsDead;
        public bool IsGloballyAlerted => alertService != null && alertService.IsAlerted;

        public void Configure(
            ActorIdentityComponent actorIdentity,
            Health actorHealth,
            NpcMotor npcMotor,
            NpcVisionSensor visionSensor,
            NpcAwareness npcAwareness,
            AlertService sceneAlertService)
        {
            identity = actorIdentity;
            health = actorHealth;
            motor = npcMotor;
            vision = visionSensor;
            awareness = npcAwareness;
            alertService = sceneAlertService;
            lastValidationError = string.Empty;
        }

        public bool TryValidate(out string error)
        {
            return TryValidate(NpcContextRequirement.All, out error);
        }

        public bool TryValidate(NpcContextRequirement requirements, out string error)
        {
            List<string> missing = null;
            AddIfMissing(requirements, NpcContextRequirement.Identity, identity, nameof(Identity), ref missing);
            AddIfMissing(requirements, NpcContextRequirement.Health, health, nameof(Health), ref missing);
            AddIfMissing(requirements, NpcContextRequirement.Motor, motor, nameof(Motor), ref missing);
            AddIfMissing(requirements, NpcContextRequirement.Vision, vision, nameof(Vision), ref missing);
            AddIfMissing(requirements, NpcContextRequirement.Awareness, awareness, nameof(Awareness), ref missing);
            AddIfMissing(requirements, NpcContextRequirement.AlertService, alertService, nameof(AlertService), ref missing);

            if (missing == null)
            {
                lastValidationError = string.Empty;
                error = string.Empty;
                return true;
            }

            error = $"{name}: NpcContext is missing required reference(s): {string.Join(", ", missing)}.";
            lastValidationError = error;
            return false;
        }

        /// <summary>Behavior nodes use this boundary to fail with a contextual Console error.</summary>
        public bool Require(NpcContextRequirement requirements, Object requester = null)
        {
            if (TryValidate(requirements, out var error))
            {
                return true;
            }

            Debug.LogError(error, requester != null ? requester : this);
            return false;
        }

        private static void AddIfMissing(
            NpcContextRequirement requested,
            NpcContextRequirement flag,
            Object reference,
            string label,
            ref List<string> missing)
        {
            if ((requested & flag) != 0 && reference == null)
            {
                missing ??= new List<string>(6);
                missing.Add(label);
            }
        }
    }
}
