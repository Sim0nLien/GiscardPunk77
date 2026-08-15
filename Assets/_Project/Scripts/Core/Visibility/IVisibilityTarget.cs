using UnityEngine;

namespace GiscardPunk77.Core
{
    public interface IVisibilityTarget
    {
        Vector3 VisibilityPoint { get; }

        bool IsCrouching { get; }
    }
}
