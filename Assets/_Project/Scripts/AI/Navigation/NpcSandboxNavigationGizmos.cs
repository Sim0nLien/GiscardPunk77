using UnityEngine;

namespace GiscardPunk77.AI.Navigation
{
    [DisallowMultipleComponent]
    public sealed class NpcSandboxNavigationGizmos : MonoBehaviour
    {
        [SerializeField] private Bounds[] walkableZones;
        [SerializeField] private Transform[] linkStarts;
        [SerializeField] private Transform[] linkEnds;
        [SerializeField] private Transform[] waitingPoints;
        [SerializeField] private Transform[] destinations;
        [SerializeField] private Transform[] firingPositions;

        public void Configure(
            Bounds[] zones,
            Transform[] starts,
            Transform[] ends,
            Transform[] waits,
            Transform[] routeDestinations,
            Transform[] firePositions)
        {
            walkableZones = zones;
            linkStarts = starts;
            linkEnds = ends;
            waitingPoints = waits;
            destinations = routeDestinations;
            firingPositions = firePositions;
        }

        private void OnDrawGizmos()
        {
            DrawWalkableZones();
            DrawLinks();
            DrawMarkers(waitingPoints, new Color(1f, 0.75f, 0.1f, 1f), 0.22f);
            DrawMarkers(destinations, new Color(0.2f, 1f, 0.25f, 1f), 0.28f);
            DrawFiringPositions();
        }

        private void DrawWalkableZones()
        {
            if (walkableZones == null)
            {
                return;
            }

            var previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.1f, 0.85f, 1f, 0.75f);

            foreach (var zone in walkableZones)
            {
                Gizmos.DrawWireCube(zone.center, zone.size);
            }

            Gizmos.matrix = previousMatrix;
        }

        private void DrawLinks()
        {
            if (linkStarts == null || linkEnds == null)
            {
                return;
            }

            Gizmos.color = new Color(0.95f, 0.15f, 1f, 1f);
            var count = Mathf.Min(linkStarts.Length, linkEnds.Length);
            for (var index = 0; index < count; index++)
            {
                var start = linkStarts[index];
                var end = linkEnds[index];
                if (start == null || end == null)
                {
                    continue;
                }

                Gizmos.DrawLine(start.position, end.position);
                Gizmos.DrawWireSphere(start.position, 0.18f);
                Gizmos.DrawWireSphere(end.position, 0.18f);
            }
        }

        private static void DrawMarkers(Transform[] markers, Color color, float radius)
        {
            if (markers == null)
            {
                return;
            }

            Gizmos.color = color;
            foreach (var marker in markers)
            {
                if (marker != null)
                {
                    Gizmos.DrawWireSphere(marker.position, radius);
                    Gizmos.DrawLine(marker.position, marker.position + Vector3.up * 0.7f);
                }
            }
        }

        private void DrawFiringPositions()
        {
            if (firingPositions == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.2f, 0.15f, 1f);
            foreach (var marker in firingPositions)
            {
                if (marker == null)
                {
                    continue;
                }

                Gizmos.DrawWireCube(marker.position, new Vector3(0.45f, 0.08f, 0.45f));
                Gizmos.DrawRay(marker.position, marker.forward * 1.2f);
            }
        }
    }
}
