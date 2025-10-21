using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Pooling")]
    [SerializeField] private GameObject originalPrefab;

    [Header("Projectile Stats")]
    [SerializeField] protected float projectileSpeed = 200f;
    [SerializeField] protected float projectileLifetime = 5f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected AnimationCurve speedOverLifetime = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
    [SerializeField] protected GameObject explosionPrefab;
    [SerializeField] private float collisionGracePeriod = 0.1f;
    
    protected Transform target;
    protected Rigidbody rb;
    protected Transform owner; 
    protected bool hasExploded = false;
    protected float age = 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) { rb = gameObject.AddComponent<Rigidbody>(); }
        rb.isKinematic = false; 
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    protected virtual void OnEnable()
    {
        ResetProjectile();
        Invoke(nameof(ReturnToPool), projectileLifetime);
    }
    
    public virtual void ResetProjectile()
    {
        age = 0f;
        hasExploded = false;
        target = null;
    }
    
    public virtual void Initialize(Transform ownerTransform, Transform initialTarget)
    {
        this.owner = ownerTransform;
        this.target = initialTarget;
    }

    protected virtual void FixedUpdate()
    {
        age += Time.fixedDeltaTime;

        float normalizedLifetime = Mathf.Clamp01(age / projectileLifetime);
        float speedMultiplier = speedOverLifetime.Evaluate(normalizedLifetime);
        rb.linearVelocity = transform.forward * projectileSpeed * speedMultiplier;
    }

    public void SetOriginalPrefab(GameObject prefab)
    {
        originalPrefab = prefab;
    }

    public void SetOwner(Transform ownerTransform) { owner = ownerTransform; }
    public void SetTarget(Transform newTarget) { target = newTarget; }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (age < collisionGracePeriod) return;
        if (owner != null && collision.transform.root == owner) return;

        Damageable damageable = collision.gameObject.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        
        Explode();
    }

    public virtual void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        ReturnToPool();
    }
    
    protected void ReturnToPool()
    {
        CancelInvoke(); 
        
        if (originalPrefab != null && ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(originalPrefab, gameObject);
        }
        else
        {
            gameObject.SetActive(false); 
        }
    }

    public void SetInitialVelocity(Vector3 velocity)
    {
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }
}

