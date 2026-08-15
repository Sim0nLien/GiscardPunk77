using GiscardPunk77.Core;
using UnityEngine;

namespace GiscardPunk77.AI.Debugging
{
    /// <summary>Inspector-controlled visibility target used only by the P07 sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class NpcVisionSandboxTarget : MonoBehaviour, IVisibilityTarget
    {
        [SerializeField] private Transform visibilityPoint;
        [SerializeField] private bool isCrouching;

        public Vector3 VisibilityPoint => visibilityPoint != null
            ? visibilityPoint.position
            : transform.position;

        public bool IsCrouching => isCrouching;

        public void Configure(Transform point, bool crouching)
        {
            visibilityPoint = point;
            isCrouching = crouching;
        }

        public void SetCrouching(bool crouching)
        {
            isCrouching = crouching;
        }
    }
}
