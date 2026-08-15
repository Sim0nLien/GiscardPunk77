using System;
using GiscardPunk77.Gameplay.Doors;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Porte coulissante activable avec la touche E.
/// A associer directement a l'objet Door.
/// </summary>
public class SlidingDoor : MonoBehaviour, IDoorPassage
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(0.1f)] private float interactionDistance = 3f;

    [Header("Mouvement")]
    [Tooltip("Direction locale dans laquelle la porte coulisse.")]
    [SerializeField] private Vector3 slideDirection = Vector3.right;
    [SerializeField, Min(0f)] private float slideDistance = 2f;
    [SerializeField, Min(0.01f)] private float slideSpeed = 3f;

    [Header("Passage commun")]
    [SerializeField] private Transform waitingPointA;
    [SerializeField] private Transform waitingPointB;
    [SerializeField, Min(0.1f)] private float reservationLifetime = 5f;
    [SerializeField, Min(0.001f)] private float passableTolerance = 0.05f;

    private InputAction interactAction;
    private Rigidbody doorBody;
    private readonly DoorReservationQueue reservations = new DoorReservationQueue();
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;
    private bool isPassable;
    private bool hasPublishedState;
    private DoorPassageState lastPublishedState;

    public bool CanUse => isActiveAndEnabled
        && doorBody != null
        && waitingPointA != null
        && waitingPointB != null;

    public bool IsPassable => CanUse && isPassable;

    public Transform WaitingPointA => waitingPointA;

    public Transform WaitingPointB => waitingPointB;

    public event Action<DoorPassageState> StateChanged;

    public event Action<object> ReservationChanged;

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
        UpdatePassability(transform.localPosition);
    }

    private void OnEnable()
    {
        interactAction?.Enable();
        PublishStateIfChanged(true);
    }

    private void OnDisable()
    {
        interactAction?.Disable();
        reservations.Clear();
        PublishStateIfChanged(true);
    }

    private void OnDestroy()
    {
        interactAction?.Dispose();
    }

    private void Update()
    {
        if (reservations.RemoveExpired(Time.time) > 0)
        {
            PublishStateIfChanged();
        }

        if (playerCamera != null && interactAction.WasPressedThisFrame() && IsPlayerLookingAtDoor())
        {
            SetOpenRequested(!isOpen);
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
            UpdatePassability(transform.localPosition);
            PublishStateIfChanged();
            return;
        }

        Vector3 nextWorldPosition = transform.parent != null
            ? transform.parent.TransformPoint(nextLocalPosition)
            : nextLocalPosition;
        doorBody.MovePosition(nextWorldPosition);
        UpdatePassability(nextLocalPosition);
        PublishStateIfChanged();
    }

    public bool RequestOpen()
    {
        if (!CanUse)
        {
            return false;
        }

        SetOpenRequested(true);
        return true;
    }

    public bool TryReserve(object owner)
    {
        if (!CanUse)
        {
            return false;
        }

        var granted = reservations.TryReserve(owner, Time.time, reservationLifetime);
        PublishStateIfChanged();
        return granted;
    }

    public bool IsReservedBy(object owner)
    {
        return reservations.IsReservedBy(owner);
    }

    public void Release(object owner)
    {
        if (reservations.Release(owner))
        {
            PublishStateIfChanged();
        }
    }

    public void ResetPassage()
    {
        reservations.Clear();
        SetOpenRequested(false);
        PublishStateIfChanged(true);
    }

    public void ConfigurePassage(Transform pointA, Transform pointB, float lifetime)
    {
        waitingPointA = pointA;
        waitingPointB = pointB;
        reservationLifetime = Mathf.Max(0.1f, lifetime);
        PublishStateIfChanged(true);
    }

    public void ConfigureMovement(Vector3 direction, float distance, float speed)
    {
        slideDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.right;
        slideDistance = Mathf.Max(0f, distance);
        slideSpeed = Mathf.Max(0.01f, speed);

        if (doorBody != null)
        {
            closedPosition = transform.localPosition;
            openPosition = closedPosition + slideDirection * slideDistance;
            UpdatePassability(transform.localPosition);
            PublishStateIfChanged(true);
        }
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

    private void OnValidate()
    {
        reservationLifetime = Mathf.Max(0.1f, reservationLifetime);
        passableTolerance = Mathf.Max(0.001f, passableTolerance);
        slideSpeed = Mathf.Max(0.01f, slideSpeed);
        slideDistance = Mathf.Max(0f, slideDistance);
    }

    private void SetOpenRequested(bool requestedOpen)
    {
        if (isOpen == requestedOpen)
        {
            return;
        }

        isOpen = requestedOpen;
        if (!requestedOpen)
        {
            isPassable = false;
        }

        PublishStateIfChanged();
    }

    private void UpdatePassability(Vector3 evaluatedLocalPosition)
    {
        isPassable = isOpen
            && (evaluatedLocalPosition - openPosition).sqrMagnitude <= passableTolerance * passableTolerance;
    }

    private void PublishStateIfChanged(bool force = false)
    {
        var state = new DoorPassageState(
            CanUse,
            isOpen,
            IsPassable,
            reservations.ActiveOwner,
            reservations.Count);

        if (!force && hasPublishedState && lastPublishedState.Equals(state))
        {
            return;
        }

        var ownerChanged = !hasPublishedState
            || !ReferenceEquals(lastPublishedState.ReservationOwner, state.ReservationOwner);
        lastPublishedState = state;
        hasPublishedState = true;
        StateChanged?.Invoke(state);

        if (ownerChanged)
        {
            ReservationChanged?.Invoke(state.ReservationOwner);
        }
    }
}
