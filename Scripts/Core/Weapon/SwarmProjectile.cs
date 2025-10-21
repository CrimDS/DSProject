using UnityEngine;

public class SwarmProjectile : HomingProjectile
{
    [Header("Swarm Properties")]
    [SerializeField] private GameObject submunitionPrefab; 
    public int submunitionCount = 8;
    public float spreadAngle = 30f;
    public float submunitionSpeed = 300f;

    [Header("Detonation")]
    [SerializeField] private float detonationTime = 3.0f;

    private Collider myCollider;

    protected override void Awake()
    {
        base.Awake();
        myCollider = GetComponent<Collider>();

        if (submunitionPrefab == null)
        {
            Debug.LogError("FATAL: SwarmProjectile prefab is missing its Submunition Prefab assignment in the Inspector!", this);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!hasExploded && age >= detonationTime)
        {
            Explode();
        }
    }

    public override void Explode()
    {
        if (hasExploded) return;
        if (submunitionPrefab == null)
        {
            base.Explode(); 
            return;
        }

        hasExploded = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        for (int i = 0; i < submunitionCount; i++)
        {
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-spreadAngle / 2, spreadAngle / 2),
                Random.Range(-spreadAngle / 2, spreadAngle / 2),
                0
            );
            
            Vector3 spawnVelocity = randomRotation * transform.forward * submunitionSpeed;
            Quaternion spawnRotation = Quaternion.LookRotation(spawnVelocity);

            GameObject submunitionGO = ObjectPool.Instance.GetFromPool(submunitionPrefab, transform.position, spawnRotation);
            Projectile submunition = submunitionGO.GetComponent<Projectile>();
            
            if (submunition != null)
            {
                submunition.Initialize(owner, null);
                submunition.SetInitialVelocity(spawnVelocity);

                Collider submunitionCollider = submunitionGO.GetComponent<Collider>();
                if (myCollider != null && submunitionCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, submunitionCollider);
                }
            }
        }

        ReturnToPool();
    }
}

