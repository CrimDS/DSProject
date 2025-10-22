using System.Linq;
using UnityEngine;

public class HomingProjectile : Projectile
{
    [Header("Homing Properties")]
    [SerializeField] protected float turnRate = 180f; 

    [Header("Turbulence Effect")]
    [SerializeField] protected float wobbleAmplitude = 1f; 
    [SerializeField] protected float wobbleFrequency = 5f;

    [Header("Authentic Behavior")]
    [SerializeField] protected float proximityFuseRadius = 10f;
    [SerializeField] private float autoTargetRange = 500f;

    // These are private and set by the weapon launcher.
    private float boostDuration;
    private float boostThrust;
    private Vector3 worldBoostDirection;
    
    protected float randomSeed;
    private float lastTargetSearchTime;
    private const float TARGET_SEARCH_INTERVAL = 0.5f;
    private float sqrProximityFuseRadius;

    protected override void Awake()
    {
        base.Awake();
        randomSeed = Random.Range(0f, 1000f);
        sqrProximityFuseRadius = proximityFuseRadius * proximityFuseRadius;
    }
    
    public override void ResetProjectile()
    {
        base.ResetProjectile();
        randomSeed = Random.Range(0f, 1000f);
    }

    // New public method for the weapon to call to start the boost.
    public void StartBoostPhase(float duration, float thrust, Vector3 worldDirection)
    {
        this.boostDuration = duration;
        this.boostThrust = thrust;
        this.worldBoostDirection = worldDirection;
    }
    
    protected override void FixedUpdate()
    {
        age += Time.fixedDeltaTime;

        if (age <= boostDuration)
        {
            // Boost phase now uses the pre-calculated world-space direction.
            float currentSpeed = projectileSpeed + boostThrust;
            rb.linearVelocity = worldBoostDirection * currentSpeed;
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity.normalized));
            }
        }
        else 
        {
            // Guidance Phase
            HandleGuidance();
        }

        // Proximity Detonation
        if (age > boostDuration && target != null)
        {
            if (proximityFuseRadius > 0 && (target.position - rb.position).sqrMagnitude < sqrProximityFuseRadius)
            {
                Explode();
                Damageable damageable = target.GetComponent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }
    
    protected void HandleGuidance()
    {
        if (target != null && !target.gameObject.activeInHierarchy)
        {
            target = null;
        }

        if (target != null)
        {
            if (owner != null && target.root == owner)
            {
                FindNewTarget();
                rb.linearVelocity = transform.forward * projectileSpeed;
                return;
            }

            Vector3 directionToTarget = (target.position - rb.position).normalized;
            
            float wobbleX = (Mathf.PerlinNoise(Time.time * wobbleFrequency, randomSeed) - 0.5f) * 2f;
            float wobbleY = (Mathf.PerlinNoise(randomSeed, Time.time * wobbleFrequency) - 0.5f) * 2f;
            Vector3 wobble = new Vector3(wobbleX, wobbleY, 0) * (wobbleAmplitude * 0.1f);
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(wobble);

            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.fixedDeltaTime));
        }
        else
        {
            FindNewTarget();
        }
         
        rb.linearVelocity = transform.forward * projectileSpeed;
    }
    
    protected void FindNewTarget()
    {
        if (Time.time - lastTargetSearchTime < TARGET_SEARCH_INTERVAL)
            return;
        
        lastTargetSearchTime = Time.time;
    
        Collider[] potentialTargets = Physics.OverlapSphere(rb.position, autoTargetRange, GameLayers.EnemiesMask);

        if (potentialTargets.Length > 0)
        {
            Transform closestTarget = potentialTargets
                .Where(c => c.GetComponent<Damageable>() != null && c.attachedRigidbody != null && c.transform.root != owner) 
                .OrderBy(c => Vector3.Distance(rb.position, c.transform.position))
                .FirstOrDefault()?.transform;
            
            if (closestTarget != null)
            {
                SetTarget(closestTarget);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityFuseRadius);
        
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, autoTargetRange);
    }
#endif
}

