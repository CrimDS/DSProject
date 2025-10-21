using UnityEngine;
using System.Collections;

public class MineProjectile : Projectile
{
    [Header("Mine Properties")]
    [SerializeField] private float armingTime = 2.0f;
    [SerializeField] private float triggerRadius = 25f;
    [SerializeField] private float spreadDuration = 1.5f;

    private SphereCollider triggerCollider;
    private bool isDrifting = true;
    private Vector3 initialDriftVelocity;

    protected override void OnEnable()
    {
        base.OnEnable();
        isDrifting = true;
        
        // This coroutine will handle setting up the colliders after the delay.
        StartCoroutine(ArmMine());
    }

    public override void Initialize(Transform ownerTransform, Transform initialTarget)
    {
        base.Initialize(ownerTransform, initialTarget);
        initialDriftVelocity = rb.linearVelocity;
    }

    protected override void FixedUpdate() 
    {
        age += Time.fixedDeltaTime;

        if (isDrifting)
        {
            if (age < spreadDuration)
            {
                rb.linearVelocity = Vector3.Lerp(initialDriftVelocity, Vector3.zero, age / spreadDuration);
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
                isDrifting = false;
            }
        }
    }

    private IEnumerator ArmMine()
    {
        // Disable colliders on spawn
        GetComponent<Collider>().enabled = false;
        
        yield return new WaitForSeconds(armingTime);

        GetComponent<Collider>().enabled = true;

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
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
        if (age > armingTime)
        {
            base.OnCollisionEnter(collision);
        }
    }
}

