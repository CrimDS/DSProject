using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A third-person camera that follows, orbits, zooms, and can switch to a free-fly mode.
/// </summary>
public class CameraController : MonoBehaviour
{
    private enum CameraMode { FollowPlayer, FollowTarget, FreeFly }

    [Header("Setup")]
    [Tooltip("The player's ship transform the camera will follow by default.")]
    public Transform playerTarget;
    [Tooltip("The targeting system to get the current enemy target from.")]
    public TargetingSystem targetingSystem;

    [Header("Follow Controls")]
    [Tooltip("How smoothly the camera follows the target's position. Lower is smoother.")]
    [Range(0.01f, 1.0f)]
    public float followSmoothness = 0.1f;
    [Tooltip("How smoothly the camera orbits. Lower is smoother.")]
    [Range(0.01f, 1.0f)]
    public float orbitSmoothness = 0.1f;
    [Tooltip("How smoothly the camera zooms. Lower is smoother.")]
    [Range(0.01f, 1.0f)]
    public float zoomSmoothness = 0.1f;
    
    [Header("Orbit & Zoom Speeds")]
    public float orbitSpeed = 200f;
    public float zoomSpeed = 50f;

    [Header("Free Fly Controls")]
    [Tooltip("How fast the camera moves in free-fly mode.")]
    public float freeFlySpeed = 50f;

    [Header("Limits")]
    public Vector2 distanceLimits = new Vector2(20f, 500f);
    public Vector2 pitchLimits = new Vector2(-20f, 80f);

    // Private state
    private CameraMode currentMode = CameraMode.FollowPlayer;
    private float targetDistance;
    private float currentDistance;
    private float targetYaw;
    private float currentYaw;
    private float targetPitch;
    private float currentPitch;

    private PlayerControls playerControls;
    private Transform currentFollowTarget;

    void Awake()
    {
        playerControls = new PlayerControls();

        // Initialize camera state
        currentFollowTarget = playerTarget;
        targetDistance = currentDistance = Vector3.Distance(transform.position, currentFollowTarget.position);
        targetYaw = currentYaw = transform.eulerAngles.y;
        targetPitch = currentPitch = transform.eulerAngles.x;
    }

    void OnEnable()
    {
        playerControls.Camera.Enable();
        playerControls.Camera.ToggleCameraMode.performed += OnToggleCameraMode;
    }

    void OnDisable()
    {
        playerControls.Camera.Disable();
        playerControls.Camera.ToggleCameraMode.performed -= OnToggleCameraMode;
    }

    private void OnToggleCameraMode(InputAction.CallbackContext context)
    {
        currentMode++;
        if (currentMode > CameraMode.FreeFly)
        {
            currentMode = CameraMode.FollowPlayer;
        }

        // When switching back to a follow mode, snap to the player ship
        if (currentMode == CameraMode.FollowPlayer)
        {
            currentFollowTarget = playerTarget;
        }
    }

    void LateUpdate()
    {
        if (currentMode == CameraMode.FreeFly)
        {
            HandleFreeFlyCamera();
        }
        else
        {
            HandleFollowCamera();
        }
    }

    private void HandleFollowCamera()
    {
        // Determine the current target based on the mode
        currentFollowTarget = (currentMode == CameraMode.FollowPlayer || targetingSystem.CurrentTarget == null) 
            ? playerTarget 
            : targetingSystem.CurrentTarget;

        if (currentFollowTarget == null) return;

        // --- Input Handling ---
        HandleOrbitAndZoomInput();

        // --- Smoothing ---
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothness);
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, orbitSmoothness);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, orbitSmoothness);

        // --- Calculate Camera Position & Rotation ---
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = currentFollowTarget.position - (rotation * Vector3.forward * currentDistance);

        // --- Apply Final Position ---
        // Removed SmoothDamp for a more direct Lerp, as requested
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness);
        transform.LookAt(currentFollowTarget.position);
    }

    private void HandleFreeFlyCamera()
    {
        // --- Input Handling ---
        HandleOrbitAndZoomInput();

        // Read movement inputs
        Vector2 moveInput = playerControls.Camera.MoveCamera.ReadValue<Vector2>();
        float verticalInput = playerControls.Camera.StrafeCameraVertical.ReadValue<float>();

        // --- Smoothing ---
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothness);
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, orbitSmoothness);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, orbitSmoothness);

        // --- Calculate Rotation & Movement ---
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x + transform.up * verticalInput).normalized;
        transform.position += moveDirection * freeFlySpeed * Time.deltaTime;
    }

    private void HandleOrbitAndZoomInput()
    {
        Vector2 mouseOrbitInput = playerControls.Camera.OrbitMouse.ReadValue<Vector2>();
        Vector2 gamepadOrbitInput = playerControls.Camera.OrbitGamepad.ReadValue<Vector2>();
        bool orbitModifierHeld = playerControls.Camera.OrbitModifier.IsPressed();

        float zoomInput = playerControls.Camera.Zoom.ReadValue<float>();
        bool isZoomingWithStick = playerControls.Camera.ZoomModifier.IsPressed();

        Vector2 orbitInput = gamepadOrbitInput;
        if (orbitInput.magnitude < 0.1f && orbitModifierHeld)
        {
            orbitInput = mouseOrbitInput;
        }

        // Handle Orbit
        bool isGamepadStickOrbit = gamepadOrbitInput.magnitude > 0.1f;
        if (isGamepadStickOrbit && isZoomingWithStick)
        {
            targetDistance -= orbitInput.y * zoomSpeed * 0.1f * Time.deltaTime;
        }
        else if (orbitInput.magnitude > 0.1f)
        {
            targetYaw += orbitInput.x * orbitSpeed * Time.deltaTime;
            targetPitch -= orbitInput.y * orbitSpeed * Time.deltaTime;
        }
        
        // Handle Mouse Wheel Zoom
        if (Mathf.Abs(zoomInput) > 0.1f)
        {
            targetDistance -= zoomInput * zoomSpeed * 0.01f;
        }

        // Apply Limits
        targetPitch = Mathf.Clamp(targetPitch, pitchLimits.x, pitchLimits.y);
        targetDistance = Mathf.Clamp(targetDistance, distanceLimits.x, distanceLimits.y);
    }
}

