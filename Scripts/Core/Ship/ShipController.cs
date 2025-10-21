using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Ship Performance Stats")]
    public float maxVelocity = 50f;
    public float maxAcceleration = 10f;
    public float maxYawRate = 90f;
    public float rollFactor = 1.0f;
    public float rollDecay = 5.0f;

    [Header("Energy Stats (Placeholder)")]
    public float maxEnergy = 1000f;
    public float energyRechargeRate = 50f;
    public float thrustEnergyCost = 20f; // Energy per second at full thrust

    // --- Public Properties ---
    public float CurrentVelocity => currentVelocity;
    public float CurrentVelocityPercent => maxVelocity > 0 ? currentVelocity / maxVelocity : 0f;
    public float CurrentHeadingDegrees => Mathf.Repeat(currentHeading, 360);
    public float TargetHeadingDegrees => Mathf.Repeat(targetHeading, 360);
    public float CurrentEnergy { get; private set; }

    // --- Private State ---
    private float targetHeading = 0f;
    private float currentHeading = 0f;
    private float targetVelocity = 0f;
    private float currentVelocity = 0f;
    private float yawInput = 0f;
    private float currentRoll = 0f;

    private Rigidbody rb;
    private const float MAX_SHIP_ROLL_DEG = 45f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; 
        
        // This is the fix for jitter. It tells Unity to smooth the Rigidbody's
        // position between physics frames, which provides a smooth position for
        // the camera in LateUpdate to follow.
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        currentHeading = transform.eulerAngles.y;
        targetHeading = currentHeading;
        CurrentEnergy = maxEnergy;
    }

    void Update()
    {
        HandleEnergy();
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        
        UpdateMovement(deltaTime);
    }

    private void HandleEnergy()
    {
        // Drain energy based on how much thrust we are requesting
        float thrustRequest = Mathf.Abs(targetVelocity - currentVelocity) / maxAcceleration;
        CurrentEnergy -= thrustRequest * thrustEnergyCost * Time.deltaTime;

        // Recharge energy
        CurrentEnergy += energyRechargeRate * Time.deltaTime;
        CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
    }
    
    private void UpdateMovement(float deltaTime)
    {
        // --- Turning & Roll ---
        if (Mathf.Abs(yawInput) > 0.01f)
        {
            targetHeading += yawInput * maxYawRate * deltaTime;
        }
        float turnDelta = Mathf.DeltaAngle(currentHeading, targetHeading);
        float turnAmount = Mathf.Clamp(turnDelta, -maxYawRate * deltaTime, maxYawRate * deltaTime);
        currentHeading += turnAmount;
        float targetRoll = -Mathf.Clamp(turnDelta * rollFactor, -MAX_SHIP_ROLL_DEG, MAX_SHIP_ROLL_DEG);
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, rollDecay * deltaTime);

        // --- Acceleration (only if we have energy) ---
        if (CurrentEnergy > 0)
        {
            float accelerationDelta = targetVelocity - currentVelocity;
            float acceleration = Mathf.Clamp(accelerationDelta, -maxAcceleration * deltaTime, maxAcceleration * deltaTime);
            currentVelocity = Mathf.Clamp(currentVelocity + acceleration, 0f, maxVelocity);
        }

        // --- Apply Movement ---
        Vector3 forwardDirection = new Vector3(Mathf.Sin(currentHeading * Mathf.Deg2Rad), 0, Mathf.Cos(currentHeading * Mathf.Deg2Rad));
        Vector3 movement = forwardDirection * (currentVelocity * deltaTime);
        rb.MovePosition(rb.position + movement);
        rb.MoveRotation(Quaternion.Euler(0, currentHeading, currentRoll));
    }

    // --- Public Control Methods ---
    public float GetTargetVelocity() => targetVelocity;
    
    public void ExecuteAllStop()
    {
        targetVelocity = 0f;
        yawInput = 0f;
        targetHeading = currentHeading;
    }

    public void SetYawInput(float yaw)
    {
        this.yawInput = Mathf.Clamp(yaw, -1f, 1f);
        if(Mathf.Abs(yaw) > 0.01f)
        {
            targetHeading = currentHeading;
        }
    }

    public void SetTargetVelocity(float velocity)
    {
        targetVelocity = Mathf.Clamp(velocity, 0, maxVelocity);
    }
}
