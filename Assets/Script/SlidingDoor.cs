using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Porte coulissante activable avec la touche E.
/// A associer directement a l'objet Door.
/// </summary>
public class SlidingDoor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(0.1f)] private float interactionDistance = 3f;

    [Header("Mouvement")]
    [Tooltip("Direction locale dans laquelle la porte coulisse.")]
    [SerializeField] private Vector3 slideDirection = Vector3.right;
    [SerializeField, Min(0f)] private float slideDistance = 2f;
    [SerializeField, Min(0.01f)] private float slideSpeed = 3f;

    private InputAction interactAction;
    private Rigidbody doorBody;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (GetComponentInChildren<Collider>() == null)
        {
            Debug.LogWarning("SlidingDoor a besoin d'un Collider pour etre visee par le joueur.", this);
        }

        // Une porte mobile est un obstacle pilote par le jeu, pas un objet soumis a la gravite.
        // Le Rigidbody cinematique permet de la deplacer sans combattre le moteur physique.
        doorBody = GetComponent<Rigidbody>();

        if (doorBody == null)
        {
            doorBody = gameObject.AddComponent<Rigidbody>();
        }

        doorBody.isKinematic = true;
        doorBody.useGravity = false;
        doorBody.interpolation = RigidbodyInterpolation.Interpolate;
        doorBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        closedPosition = transform.localPosition;
        Vector3 direction = slideDirection.sqrMagnitude > 0f
            ? slideDirection.normalized
            : Vector3.right;
        openPosition = closedPosition + direction * slideDistance;

        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
    }

    private void OnEnable()
    {
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.Disable();
    }

    private void OnDestroy()
    {
        interactAction?.Dispose();
    }

    private void Update()
    {
        if (playerCamera != null && interactAction.WasPressedThisFrame() && IsPlayerLookingAtDoor())
        {
            isOpen = !isOpen;
        }
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;
        Vector3 nextLocalPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            slideSpeed * Time.fixedDeltaTime);

        if ((nextLocalPosition - transform.localPosition).sqrMagnitude < 0.000001f)
        {
            return;
        }

        Vector3 nextWorldPosition = transform.parent != null
            ? transform.parent.TransformPoint(nextLocalPosition)
            : nextLocalPosition;
        doorBody.MovePosition(nextWorldPosition);
    }

    private bool IsPlayerLookingAtDoor()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return hit.transform == transform || hit.transform.IsChildOf(transform);
    }
}
