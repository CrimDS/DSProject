using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A third-person camera that follows, orbits, and zooms around a target.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The target the camera will follow.")]
    public Transform target;

    [Header("Controls")]
    [Tooltip("How far the camera is from the target.")]
    public float distance = 100f;
    [Tooltip("How fast the camera orbits with mouse/stick movement.")]
    public float orbitSpeed = 200f;
    [Tooltip("How fast the camera zooms with the mouse wheel/controller.")]
    public float zoomSpeed = 50f;
    [Tooltip("How smoothly the camera follows the target.")]
    public float followDamping = 5f;

    [Header("Limits")]
    [Tooltip("The minimum and maximum follow distance.")]
    public Vector2 distanceLimits = new Vector2(20f, 500f);
    [Tooltip("The minimum and maximum camera pitch angle in degrees.")]
    public Vector2 pitchLimits = new Vector2(-20f, 80f);

    // Private state
    private float yaw = 0f;
    private float pitch = 20f;
    private Vector3 currentVelocity;

    private PlayerControls playerControls;

    void Awake()
    {
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        playerControls.Camera.Enable();
    }

    void OnDisable()
    {
        playerControls.Camera.Disable();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- Input Handling ---
        Vector2 mouseOrbitInput = playerControls.Camera.OrbitMouse.ReadValue<Vector2>();
        Vector2 gamepadOrbitInput = playerControls.Camera.OrbitGamepad.ReadValue<Vector2>();
        bool orbitModifierHeld = playerControls.Camera.OrbitModifier.IsPressed();

        float zoomInput = playerControls.Camera.Zoom.ReadValue<float>();
        bool isZoomingWithStick = playerControls.Camera.ZoomModifier.IsPressed();

        // --- Determine final orbit input ---
        // Prioritize gamepad input. If it's not being used, check for mouse input with the modifier.
        Vector2 orbitInput = gamepadOrbitInput;
        if (orbitInput.magnitude < 0.1f && orbitModifierHeld)
        {
            orbitInput = mouseOrbitInput;
        }


        // Handle Orbit
        if (orbitInput.magnitude > 0.1f)
        {
            // The zoom-with-stick logic is specific to the gamepad's right stick
            bool isGamepadStickOrbit = gamepadOrbitInput.magnitude > 0.1f;
            if (isGamepadStickOrbit && isZoomingWithStick)
            {
                distance -= orbitInput.y * zoomSpeed * 0.1f * Time.deltaTime;
            }
            else
            {
                yaw += orbitInput.x * orbitSpeed * Time.deltaTime;
                pitch -= orbitInput.y * orbitSpeed * Time.deltaTime;
            }
        }
        
        // Handle Mouse Wheel Zoom
        if (Mathf.Abs(zoomInput) > 0.1f)
        {
            distance -= zoomInput * zoomSpeed * 0.01f;
        }

        // --- Apply Limits ---
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        distance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);

        // --- Calculate Camera Position & Rotation ---
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 targetPosition = target.position - (rotation * Vector3.forward * distance);

        // --- Apply Smoothing ---
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, followDamping * Time.deltaTime);
        transform.LookAt(target.position);
    }
}

