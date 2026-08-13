using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float speed = 4f;
    [SerializeField, Min(0f)] private float sprintSpeed = 6f;
    [SerializeField, Min(0f)] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = -19.62f;
    [SerializeField, Min(0f)] private float jumpHeight = 2f;

    [Header("Crouch")]
    [SerializeField, Min(0.1f)] private float crouchedHeight = 1f;
    [SerializeField, Min(0f)] private float crouchTransitionSpeed = 8f;

    [Header("FPS Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Physics Interaction")]
    [SerializeField, Min(0f)] private float pushStrength = 18f;
    [SerializeField, Min(0f)] private float maxPushSpeed = 2.5f;

    private CharacterController controller;
    private FpsCameraController cameraController;
    private Vector3 velocity;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private float standingHeight;
    private Vector3 standingCenter;
    private float standingEyeHeight;
    private Vector3 horizontalMoveVelocity;
    private bool isCrouching;

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        DisableDuplicatePlayerColliders();
        SetupFpsCamera();

        standingHeight = controller.height;
        standingCenter = controller.center;
        crouchedHeight = Mathf.Clamp(crouchedHeight, controller.radius * 2f, standingHeight);
        standingEyeHeight = cameraController != null
            ? cameraController.EyeHeight
            : standingHeight * 0.75f;

        // Movement: WASD/arrows and the left gamepad stick.
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");

        crouchAction = new InputAction("Crouch", binding: "<Keyboard>/leftCtrl");
        crouchAction.AddBinding("<Keyboard>/c");
        crouchAction.AddBinding("<Gamepad>/rightStickPress");
    }

    private void DisableDuplicatePlayerColliders()
    {
        foreach (Collider playerCollider in GetComponents<Collider>())
        {
            if (playerCollider != controller && playerCollider.enabled)
            {
                playerCollider.enabled = false;
                Debug.LogWarning(
                    $"Collider en double desactive sur le joueur : {playerCollider.GetType().Name}. " +
                    "Le CharacterController gere deja les collisions.",
                    playerCollider);
            }
        }
    }

    private void SetupFpsCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogError("PlayerController needs a camera tagged MainCamera.", this);
            return;
        }

        cameraController = playerCamera.GetComponent<FpsCameraController>();

        if (cameraController == null)
        {
            cameraController = playerCamera.gameObject.AddComponent<FpsCameraController>();
        }

        cameraController.SetPlayer(transform);
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        crouchAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
    }

    private void Update()
    {
        UpdateCrouch();

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (jumpAction.WasPressedThisFrame() && controller.isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        float currentSpeed = isCrouching
            ? crouchSpeed
            : sprintAction.IsPressed() ? sprintSpeed : speed;
        horizontalMoveVelocity = move * currentSpeed;
        Vector3 finalMove = horizontalMoveVelocity + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    private void UpdateCrouch()
    {
        bool wantsToCrouch = crouchAction.IsPressed();

        if (wantsToCrouch)
        {
            isCrouching = true;
        }
        else if (isCrouching && CanStandUp())
        {
            isCrouching = false;
        }

        float targetHeight = isCrouching ? crouchedHeight : standingHeight;
        float newHeight = Mathf.MoveTowards(
            controller.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime);

        float standingBottom = standingCenter.y - standingHeight * 0.5f;
        Vector3 newCenter = standingCenter;
        newCenter.y = standingBottom + newHeight * 0.5f;
        controller.height = newHeight;
        controller.center = newCenter;

        if (cameraController != null)
        {
            float crouchedEyeHeight = Mathf.Max(
                0.1f,
                standingEyeHeight - (standingHeight - crouchedHeight));
            float heightRange = Mathf.Max(0.001f, standingHeight - crouchedHeight);
            float crouchAmount = (standingHeight - newHeight) / heightRange;
            cameraController.SetEyeHeight(
                Mathf.Lerp(standingEyeHeight, crouchedEyeHeight, crouchAmount));
        }
    }

    private bool CanStandUp()
    {
        float radius = controller.radius * Mathf.Max(
            Mathf.Abs(transform.lossyScale.x),
            Mathf.Abs(transform.lossyScale.z));
        float scaledHeight = standingHeight * Mathf.Abs(transform.lossyScale.y);
        float halfSegment = Mathf.Max(0f, scaledHeight * 0.5f - radius);
        Vector3 worldCenter = transform.TransformPoint(standingCenter);
        Vector3 bottom = worldCenter - transform.up * halfSegment;
        Vector3 top = worldCenter + transform.up * halfSegment;
        Collider[] overlaps = Physics.OverlapCapsule(
            bottom,
            top,
            radius,
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (overlap != controller && overlap.transform.root != transform.root)
            {
                return false;
            }
        }

        return true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody hitBody = hit.collider.attachedRigidbody;

        if (hitBody == null || hitBody.isKinematic || !controller.isGrounded)
        {
            return;
        }

        // Ignore mostly downward hits so we do not shove objects by landing on them.
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

        if (pushDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        pushDirection.Normalize();

        float approachSpeed = Vector3.Dot(horizontalMoveVelocity, pushDirection);

        if (approachSpeed <= 0.05f)
        {
            return;
        }

        float speedRatio = Mathf.Clamp01(approachSpeed / Mathf.Max(0.01f, sprintSpeed));
        Vector3 bodyHorizontalVelocity = Vector3.ProjectOnPlane(hitBody.linearVelocity, Vector3.up);
        float currentPushSpeed = Mathf.Max(0f, Vector3.Dot(bodyHorizontalVelocity, pushDirection));

        if (currentPushSpeed >= maxPushSpeed)
        {
            return;
        }

        float targetPushSpeed = maxPushSpeed * speedRatio;
        float missingSpeed = Mathf.Max(0f, targetPushSpeed - currentPushSpeed);
        float velocityChange = Mathf.Min(missingSpeed, pushStrength * Time.deltaTime);

        // Le CharacterController n'applique aucune force physique par lui-meme.
        // On augmente donc doucement la vitesse de la caisse jusqu'a une limite,
        // ce qui permet de pousser aussi les caisses lourdes sans les faire exploser.
        hitBody.AddForce(
            pushDirection * velocityChange,
            ForceMode.VelocityChange);
    }
}
