using UnityEngine;

/// <summary>
/// Controls a Particle System to create a responsive engine trail effect.
/// The trail's length and intensity are based on the ship's velocity.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class EngineTrail : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ShipController to get velocity information from.")]
    public ShipController shipController;

    [Header("Trail Settings")]
    [Tooltip("The particle speed when the ship is at maximum velocity.")]
    public float maxSpeed = 20f;
    [Tooltip("The particle emission rate when the ship is at maximum velocity.")]
    public float maxEmissionRate = 100f;

    private ParticleSystem ps;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        mainModule = ps.main;
        emissionModule = ps.emission;

        if (shipController == null)
        {
            // Try to find the ShipController on the root object if not assigned
            shipController = GetComponentInParent<ShipController>();
            if (shipController == null)
            {
                Debug.LogError("EngineTrail: ShipController not found! Please assign it in the Inspector.", this);
                enabled = false; // Disable the script if the controller is missing
            }
        }
    }

    void Update()
    {
        if (shipController == null)
            return;

        // Get the current velocity as a percentage of max velocity (from 0 to 1)
        float velocityPercent = shipController.CurrentVelocityPercent;

        // Adjust the particle system based on the velocity percentage
        mainModule.startSpeed = Mathf.Lerp(0, maxSpeed, velocityPercent);
        emissionModule.rateOverTime = Mathf.Lerp(0, maxEmissionRate, velocityPercent);
    }
}
