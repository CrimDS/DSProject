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

    protected override void OnEnable()
    {
        base.OnEnable();
        isArmed = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        StartCoroutine(ArmMine());
    }

    protected override void FixedUpdate() 
    {
        age += Time.fixedDeltaTime;

        if (isArmed && target != null)
        {
            Vector3 directionToTarget = (target.position - rb.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.fixedDeltaTime));
            
            rb.linearVelocity = transform.forward * projectileSpeed;
        }
    }

    private IEnumerator ArmMine()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(armingTime);
        isArmed = true;
        
        GetComponent<Collider>().enabled = true;

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
    }

    void OnTriggerEnter(Collider other)
    {
        if (target != null || !isArmed || hasExploded || other.isTrigger) return;

        if (other.gameObject.layer == GameLayers.Enemies)
        {
            if (owner != null && other.transform.root == owner) return;
            
            target = other.transform;
            
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
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

