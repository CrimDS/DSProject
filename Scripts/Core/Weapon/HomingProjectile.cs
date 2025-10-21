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

    public float BoostDuration { get; set; } = 0.5f;
    public float BoostThrust { get; set; } = 50f;
    public Vector3 BoostDirection { get; set; } = Vector3.forward;
    public BoostSpace BoostLaunchSpace { get; set; } = BoostSpace.Local;

    protected float randomSeed;
    
    private float lastTargetSearchTime;
    private const float TARGET_SEARCH_INTERVAL = 0.5f;
    private float sqrProximityFuseRadius;

    public enum BoostSpace { Local, World }

    protected override void Awake()
    {
        base.Awake();
        sqrProximityFuseRadius = proximityFuseRadius * proximityFuseRadius;
    }

    public override void ResetProjectile()
    {
        base.ResetProjectile();
        randomSeed = Random.Range(0f, 1000f);
    }
    
    protected override void FixedUpdate()
    {
        age += Time.fixedDeltaTime;

        if (age <= BoostDuration)
        {
            // Boost Phase
            float currentSpeed = projectileSpeed + BoostThrust;
            Vector3 worldBoostDirection = BoostLaunchSpace == BoostSpace.Local 
                ? (transform.parent != null ? transform.parent.TransformDirection(BoostDirection.normalized) : transform.TransformDirection(BoostDirection.normalized)) 
                : BoostDirection.normalized;

            rb.linearVelocity = worldBoostDirection * currentSpeed;
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity.normalized));
            }
        }
        else 
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

        if (age > BoostDuration && target != null)
        {
            if (sqrProximityFuseRadius > 0 && (target.position - rb.position).sqrMagnitude < sqrProximityFuseRadius)
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

