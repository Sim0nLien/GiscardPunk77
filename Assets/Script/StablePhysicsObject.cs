using UnityEngine;

/// <summary>
/// Reglage standard pour un objet physique que le joueur peut pousser/deplacer.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class StablePhysicsObject : MonoBehaviour
{
    [Header("Physics Preset")]
    [SerializeField] private bool configureOnAwake = true;
    [SerializeField] private bool snapToGroundOnStart = true;
    [SerializeField] private bool freezePitchAndRoll = true;
    [SerializeField] private RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
    [SerializeField] private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    [SerializeField, Min(0f)] private float linearDamping = 1.2f;
    [SerializeField, Min(0f)] private float angularDamping = 3f;

    [Header("Ground Snap")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField, Min(0f)] private float groundOffset = 0.01f;

    private Rigidbody body;
    private GroundSnapItem groundSnap;

    private void Awake()
    {
        CacheComponents();

        if (configureOnAwake)
        {
            ApplyPreset();
        }
    }

    private void Start()
    {
        if (snapToGroundOnStart)
        {
            SnapToGround();
        }
    }

    public void ApplyPreset()
    {
        CacheRigidbody();

        body.useGravity = true;
        body.isKinematic = false;
        body.interpolation = interpolation;
        body.collisionDetectionMode = collisionDetectionMode;
        body.linearDamping = linearDamping;
        body.angularDamping = angularDamping;

        if (freezePitchAndRoll)
        {
            body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public bool SnapToGround()
    {
        CacheRigidbody();
        EnsureGroundSnap();
        groundSnap.Configure(groundLayers, groundOffset, true);
        return groundSnap.SnapToGround();
    }

    private void CacheComponents()
    {
        CacheRigidbody();

        if (snapToGroundOnStart)
        {
            EnsureGroundSnap();
        }
    }

    private void CacheRigidbody()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
    }

    private void EnsureGroundSnap()
    {
        if (groundSnap == null)
        {
            groundSnap = GetComponent<GroundSnapItem>();

            if (groundSnap == null)
            {
                groundSnap = gameObject.AddComponent<GroundSnapItem>();
            }
        }
    }
}
