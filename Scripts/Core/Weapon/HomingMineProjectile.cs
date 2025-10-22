using UnityEngine;
using System.Collections;

public class HomingMineProjectile : Projectile
{
    [Header("Mine Properties")]
    [SerializeField] private float armingTime = 3.0f;
    [SerializeField] private float triggerRadius = 100f;
    [SerializeField] private float turnRate = 120f;
    [SerializeField] private float acceleration = 50f;

    private SphereCollider triggerCollider;
    private bool isArmed = false;

    protected override void Awake()
    {
        base.Awake(); 
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    protected override void Start()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        // This Invoke replaces the Destroy call in the base Start method.
        Invoke(nameof(ReturnToPool), 180f); 
        
        StartCoroutine(ArmMine());
    }

    protected override void FixedUpdate() 
    {
        age += Time.fixedDeltaTime; // We still need to track age.

        if (isArmed && target != null)
        {
            Vector3 directionToTarget = (target.position - rb.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.fixedDeltaTime));
            
            // Use AddForce for a more realistic acceleration.
            rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);

            // Clamp the velocity to the maximum speed set in the Inspector.
            if (rb.linearVelocity.magnitude > projectileSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * projectileSpeed;
            }
        }
        else
        {
            // Stay stationary until armed and a target is acquired.
            rb.linearVelocity = Vector3.zero;
        }
    }

    private IEnumerator ArmMine()
    {
        yield return new WaitForSeconds(armingTime);
        isArmed = true;
        
        Collider mainCollider = GetComponent<Collider>();
        if(mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
    }

    void OnTriggerEnter(Collider other)
    {
        if (target != null || !isArmed || hasExploded || other.isTrigger) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            if (owner != null && other.transform.root == owner) return;
            
            if (other.GetComponent<Damageable>() != null)
            {
                Debug.Log($"HOMING MINE '{gameObject.name}' acquiring new target: {other.gameObject.name}", this);
                target = other.transform;
            
                if (triggerCollider != null)
                {
                    triggerCollider.enabled = false;
                }
            }
        }
    }

    protected override void OnCollisionEnter(Collision collision) 
    {
        if (isArmed)
        {
            base.OnCollisionEnter(collision);
        }
    }
}

