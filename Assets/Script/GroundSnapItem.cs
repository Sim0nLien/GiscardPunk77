using System;
using UnityEngine;

/// <summary>
/// Recale un objet sur le sol en utilisant le bas de ses colliders.
/// A mettre sur l'objet racine d'un item/prop posable.
/// </summary>
public class GroundSnapItem : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private bool snapOnStart = true;
    [SerializeField] private float rayStartHeight = 2f;
    [SerializeField] private float rayDistance = 8f;
    [SerializeField] private float groundOffset = 0.01f;
    [SerializeField] private float maxSnapUpDistance = 0.5f;
    [SerializeField] private bool stopRigidbodyWhenSnapping = true;

    private Collider[] itemColliders;

    private void Awake()
    {
        RefreshColliders();
    }

    private void Start()
    {
        if (snapOnStart)
        {
            SnapToGround();
        }
    }

    public bool SnapToGround()
    {
        RefreshColliders();
        Physics.SyncTransforms();

        if (!TryGetSolidBounds(out Bounds itemBounds))
        {
            Debug.LogWarning("GroundSnapItem a besoin d'au moins un Collider non-trigger.", this);
            return false;
        }

        if (!TryFindSupportHeight(itemBounds, out float supportHeight))
        {
            return false;
        }

        float pivotToBottom = transform.position.y - itemBounds.min.y;
        Vector3 snappedPosition = transform.position;
        snappedPosition.y = supportHeight + pivotToBottom + groundOffset;
        MoveObject(snappedPosition);
        return true;
    }

    public void Configure(LayerMask layers, float offset, bool stopRigidbody)
    {
        groundLayers = layers;
        groundOffset = offset;
        stopRigidbodyWhenSnapping = stopRigidbody;
    }

    private void RefreshColliders()
    {
        itemColliders = GetComponentsInChildren<Collider>();
    }

    private bool TryFindSupportHeight(Bounds itemBounds, out float supportHeight)
    {
        supportHeight = float.NegativeInfinity;
        bool foundSupport = false;

        foreach (Vector3 footprintPoint in GetFootprintPoints(itemBounds))
        {
            Vector3 rayOrigin = new Vector3(
                footprintPoint.x,
                itemBounds.max.y + rayStartHeight,
                footprintPoint.z);

            float castDistance = rayStartHeight + itemBounds.size.y + rayDistance + maxSnapUpDistance;
            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                castDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            foreach (RaycastHit hit in hits)
            {
                if (IsOwnCollider(hit.collider))
                {
                    continue;
                }

                // Evite de se recaler sur un objet situe franchement au-dessus de l'item.
                if (hit.point.y > itemBounds.min.y + maxSnapUpDistance)
                {
                    continue;
                }

                supportHeight = Mathf.Max(supportHeight, hit.point.y);
                foundSupport = true;
                break;
            }
        }

        return foundSupport;
    }

    private Vector3[] GetFootprintPoints(Bounds itemBounds)
    {
        float insetX = Mathf.Min(itemBounds.extents.x * 0.75f, 0.25f);
        float insetZ = Mathf.Min(itemBounds.extents.z * 0.75f, 0.25f);
        float y = itemBounds.center.y;

        return new[]
        {
            new Vector3(itemBounds.center.x, y, itemBounds.center.z),
            new Vector3(itemBounds.min.x + insetX, y, itemBounds.min.z + insetZ),
            new Vector3(itemBounds.max.x - insetX, y, itemBounds.min.z + insetZ),
            new Vector3(itemBounds.min.x + insetX, y, itemBounds.max.z - insetZ),
            new Vector3(itemBounds.max.x - insetX, y, itemBounds.max.z - insetZ)
        };
    }

    private void MoveObject(Vector3 snappedPosition)
    {
        if (TryGetComponent(out Rigidbody itemBody))
        {
            itemBody.position = snappedPosition;

            if (stopRigidbodyWhenSnapping)
            {
                itemBody.linearVelocity = Vector3.zero;
                itemBody.angularVelocity = Vector3.zero;
                itemBody.Sleep();
            }

            Physics.SyncTransforms();
            return;
        }

        transform.position = snappedPosition;
        Physics.SyncTransforms();
    }

    private bool TryGetSolidBounds(out Bounds bounds)
    {
        bounds = default;
        bool hasBounds = false;

        foreach (Collider itemCollider in itemColliders)
        {
            if (itemCollider == null || itemCollider.isTrigger || !itemCollider.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = itemCollider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(itemCollider.bounds);
            }
        }

        return hasBounds;
    }

    private bool IsOwnCollider(Collider hitCollider)
    {
        foreach (Collider itemCollider in itemColliders)
        {
            if (hitCollider == itemCollider)
            {
                return true;
            }
        }

        return false;
    }
}
