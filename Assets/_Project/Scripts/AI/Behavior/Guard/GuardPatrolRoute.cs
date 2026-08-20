using UnityEngine;

namespace GiscardPunk77.AI.Behavior.Guard
{
    /// <summary>
    /// Explicit patrol points authored as offsets from the guard's captured post.
    /// They never follow the moving capsule Transform after initialization.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GuardPatrolRoute : MonoBehaviour
    {
        [SerializeField] private Vector3[] postLocalPoints =
        {
            new(0f, 0f, 3f),
            new(3f, 0f, 3f),
            new(3f, 0f, 0f)
        };

        private Vector3 postPosition;
        private Quaternion postRotation = Quaternion.identity;
        private bool initialized;

        public int Count => postLocalPoints?.Length ?? 0;

        public void Configure(params Vector3[] localPoints)
        {
            postLocalPoints = localPoints ?? System.Array.Empty<Vector3>();
        }

        public void Initialize(Vector3 worldPostPosition, Quaternion worldPostRotation)
        {
            postPosition = worldPostPosition;
            postRotation = worldPostRotation;
            initialized = true;
        }

        public bool TryGetWorldPoint(int index, out Vector3 point)
        {
            if (index < 0 || index >= Count)
            {
                point = default;
                return false;
            }

            var originPosition = initialized ? postPosition : transform.position;
            var originRotation = initialized ? postRotation : transform.rotation;
            point = originPosition + originRotation * postLocalPoints[index];
            return true;
        }

        private void Awake()
        {
            Initialize(transform.position, transform.rotation);
        }

        private void OnDrawGizmosSelected()
        {
            if (Count == 0)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
            var previous = transform.position;
            for (var index = 0; index < Count; index++)
            {
                var point = transform.position + transform.rotation * postLocalPoints[index];
                Gizmos.DrawWireSphere(point, 0.2f);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }

            Gizmos.DrawLine(previous, transform.position);
        }
    }
}
