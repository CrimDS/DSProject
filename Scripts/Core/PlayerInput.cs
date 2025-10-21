using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input for ship controls (flight and weapons).
/// </summary>
[RequireComponent(typeof(ShipController))]
[RequireComponent(typeof(WeaponSystem))]
[RequireComponent(typeof(TargetingSystem))]
public class PlayerInput : MonoBehaviour
{
    private ShipController shipController;
    private WeaponSystem weaponSystem;
    private TargetingSystem targetingSystem;
    private PlayerControls playerControls;

    void Awake()
    {
        shipController = GetComponent<ShipController>();
        weaponSystem = GetComponent<WeaponSystem>();
        targetingSystem = GetComponent<TargetingSystem>();
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        playerControls.Gameplay.Enable();

        // Register actions
        playerControls.Gameplay.AlphaStrike.performed += OnAlphaStrike;
        playerControls.Gameplay.FireGroup1.performed += ctx => OnFireGroup(1);
        playerControls.Gameplay.FireGroup2.performed += ctx => OnFireGroup(2);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(3);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(4);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(5);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(6);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(7);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(8);
        playerControls.Gameplay.FireGroup3.performed += ctx => OnFireGroup(9);
        playerControls.Gameplay.CycleTarget.performed += OnCycleTarget;
    }

    void OnDisable()
    {
        playerControls.Gameplay.Disable();
        
        // It's good practice to unregister all events
        playerControls.Gameplay.AlphaStrike.performed -= OnAlphaStrike;
        playerControls.Gameplay.FireGroup1.performed -= ctx => OnFireGroup(1);
        playerControls.Gameplay.FireGroup2.performed -= ctx => OnFireGroup(2);
        playerControls.Gameplay.FireGroup3.performed -= ctx => OnFireGroup(3);
        playerControls.Gameplay.FireGroup1.performed -= ctx => OnFireGroup(4);
        playerControls.Gameplay.FireGroup2.performed -= ctx => OnFireGroup(5);
        playerControls.Gameplay.FireGroup3.performed -= ctx => OnFireGroup(6);
        playerControls.Gameplay.FireGroup1.performed -= ctx => OnFireGroup(7);
        playerControls.Gameplay.FireGroup2.performed -= ctx => OnFireGroup(8);
        playerControls.Gameplay.FireGroup3.performed -= ctx => OnFireGroup(9);    
        playerControls.Gameplay.CycleTarget.performed -= OnCycleTarget;
    }

    void Update()
    {
        HandleMovement();
        weaponSystem.SetTarget(targetingSystem.CurrentTarget);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = playerControls.Gameplay.Move.ReadValue<Vector2>();
        shipController.SetYawInput(moveInput.x);

        if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            float currentTargetVelocity = shipController.GetTargetVelocity();
            float newTargetVelocity = currentTargetVelocity + moveInput.y * shipController.maxAcceleration * Time.deltaTime;
            shipController.SetTargetVelocity(newTargetVelocity);
        }

        if (playerControls.Gameplay.AllStop.triggered)
        {
            shipController.ExecuteAllStop();
        }
    }
    
    private void OnAlphaStrike(InputAction.CallbackContext context)
    {
        weaponSystem.FireAlphaStrike();
    }

    private void OnFireGroup(int groupNumber)
    {
        weaponSystem.FireGroup(groupNumber);
    }

    private void OnCycleTarget(InputAction.CallbackContext context)
    {
        targetingSystem.CycleTarget();
    }
}

