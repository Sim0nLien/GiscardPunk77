using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Prototype d'arme FPS a placer sur une capsule.
/// L'arme est attachee a la camera et tire de petites capsules physiques.
/// </summary>
public class CapsuleWeapon : MonoBehaviour
{
    [Header("Placement dans la main")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private bool attachToCamera = true;
    [SerializeField] private Vector3 handPosition = new Vector3(0.35f, -0.25f, 0.65f);
    [SerializeField] private Vector3 handRotation = new Vector3(0f, 0f, -8f);

    [Header("Tir")]
    [Tooltip("Optionnel. Le projectile apparait ici. Sans point de tir, il apparait devant l'arme.")]
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0.01f)] private float shotsPerSecond = 5f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 30f;
    [SerializeField, Min(0.01f)] private float projectileSize = 0.08f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 5f;
    [SerializeField] private bool useGravity;
    [SerializeField] private Color projectileColor = new Color(1f, 0.45f, 0.05f);

    private InputAction fireAction;
    private float nextShotTime;
    private Collider[] playerColliders;
    private MaterialPropertyBlock projectileProperties;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogError("CapsuleWeapon a besoin d'une camera avec le tag MainCamera.", this);
            enabled = false;
            return;
        }

        if (attachToCamera)
        {
            transform.SetParent(playerCamera.transform, false);
            transform.localPosition = handPosition;
            transform.localRotation = Quaternion.Euler(handRotation);
        }

        // Une arme tenue ne doit pas pousser le joueur ni bloquer les projectiles.
        foreach (Collider weaponCollider in GetComponentsInChildren<Collider>())
        {
            weaponCollider.enabled = false;
        }

        playerColliders = playerCamera.transform.root.GetComponentsInChildren<Collider>();

        projectileProperties = new MaterialPropertyBlock();
        projectileProperties.SetColor("_BaseColor", projectileColor);
        projectileProperties.SetColor("_Color", projectileColor);

        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");
    }

    private void OnEnable()
    {
        fireAction?.Enable();
    }

    private void OnDisable()
    {
        fireAction?.Disable();
    }

    private void OnDestroy()
    {
        fireAction?.Dispose();
    }

    private void Update()
    {
        if (fireAction.WasPressedThisFrame() && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / shotsPerSecond;
        }
    }

    private void Shoot()
    {
        Vector3 direction = playerCamera.transform.forward;
        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position + direction * 0.45f;

        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        projectile.name = "CapsuleProjectile";
        projectile.transform.SetPositionAndRotation(
            spawnPosition,
            Quaternion.FromToRotation(Vector3.up, direction));
        projectile.transform.localScale = Vector3.one * projectileSize;

        Renderer projectileRenderer = projectile.GetComponent<Renderer>();
        projectileRenderer.SetPropertyBlock(projectileProperties);

        Collider projectileCollider = projectile.GetComponent<Collider>();
        foreach (Collider playerCollider in playerColliders)
        {
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider);
            }
        }

        Rigidbody projectileBody = projectile.AddComponent<Rigidbody>();
        projectileBody.useGravity = useGravity;
        projectileBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        projectileBody.linearVelocity = direction * projectileSpeed;

        CapsuleProjectile capsuleProjectile = projectile.AddComponent<CapsuleProjectile>();
        capsuleProjectile.SetLifetime(projectileLifetime);
    }
}
