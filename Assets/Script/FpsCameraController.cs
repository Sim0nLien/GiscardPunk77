using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Camera))]
public class FpsCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private float eyeHeight = 0.75f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
    [SerializeField, Min(0f)] private float gamepadSensitivity = 180f;
    [SerializeField, Range(0f, 89f)] private float verticalLookLimit = 85f;

    private InputAction mouseLookAction;
    private InputAction gamepadLookAction;
    private float pitch;
    private bool cursorLocked;

    public float EyeHeight => eyeHeight;

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        FollowPlayer();
    }

    public void SetEyeHeight(float newEyeHeight)
    {
        eyeHeight = Mathf.Max(0.1f, newEyeHeight);
    }

    private void Awake()
    {
        mouseLookAction = new InputAction("Mouse Look", binding: "<Mouse>/delta");
        gamepadLookAction = new InputAction("Gamepad Look", binding: "<Gamepad>/rightStick");

        pitch = NormalizeAngle(transform.localEulerAngles.x);
        SetCursorLocked(true);
    }

    private void OnEnable()
    {
        mouseLookAction.Enable();
        gamepadLookAction.Enable();
    }

    private void OnDisable()
    {
        mouseLookAction.Disable();
        gamepadLookAction.Disable();
        SetCursorLocked(false);
    }

    private void Update()
    {
        HandleCursorLock();

        if (!cursorLocked || player == null)
        {
            return;
        }

        Vector2 mouseDelta = mouseLookAction.ReadValue<Vector2>() * mouseSensitivity;
        Vector2 gamepadDelta = gamepadLookAction.ReadValue<Vector2>()
            * gamepadSensitivity
            * Time.deltaTime;
        Vector2 lookDelta = mouseDelta + gamepadDelta;

        player.Rotate(Vector3.up, lookDelta.x, Space.Self);

        pitch = Mathf.Clamp(pitch - lookDelta.y, -verticalLookLimit, verticalLookLimit);
        transform.rotation = Quaternion.Euler(pitch, player.eulerAngles.y, 0f);
    }

    private void LateUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (player != null)
        {
            transform.position = player.position + Vector3.up * eyeHeight;
        }
    }

    private void HandleCursorLock()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(false);
        }
        else if (!cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLocked(true);
        }
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
