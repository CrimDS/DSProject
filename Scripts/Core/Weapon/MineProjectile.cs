using UnityEngine;
using System.Collections;

/// <summary>
/// A stationary mine that arms after a delay and explodes when an enemy gets close.
/// </summary>
public class MineProjectile : Projectile
{
    [Header("Mine Properties")]
    [Tooltip("The delay in seconds before the mine becomes active.")]
    [SerializeField] private float armingTime = 2.0f;
    [Tooltip("The radius of the trigger that detects nearby enemies.")]
    [SerializeField] private float triggerRadius = 25f;

    private SphereCollider triggerCollider;

    // --- THE CHANGE ---
    // The Start and FixedUpdate methods are now removed. This mine is completely stationary
    // and inherits all necessary setup from the base Projectile's OnEnable method.
    // The Initialize method from the parent will be called, but since we set speed to 0, it won't move.

    private IEnumerator ArmMine()
    {
        yield return new WaitForSeconds(armingTime);

        // After the arming delay, enable a large trigger collider.
        Collider mainCollider = GetComponent<Collider>();
        if(mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
    }

    // This is called when the projectile is taken from the pool.
    public override void Initialize(Transform ownerTransform, Transform initialTarget)
    {
        base.Initialize(ownerTransform, initialTarget);
        // Ensure the mine is stationary upon initialization.
        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Start the arming sequence when the mine is initialized.
        StartCoroutine(ArmMine());
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded || other.isTrigger) return;

        if (other.gameObject.layer == GameLayers.Enemies)
        {
            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Explode();
            }
        }
    }
    
    protected override void OnCollisionEnter(Collision collision) 
    {
        // Only trigger collision logic if the mine is armed.
        if (age > armingTime)
        {
            base.OnCollisionEnter(collision);
        }
    }
}

