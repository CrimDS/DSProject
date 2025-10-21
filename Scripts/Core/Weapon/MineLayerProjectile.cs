using UnityEngine;

public class MineLayerProjectile : HomingProjectile
{
    [Header("Mine Layer Properties")]
    public GameObject minePrefab;
    public int mineCount = 4;
    public float spreadSpeed = 15f;

    [Header("Detonation")]
    [SerializeField] private float detonationTime = 5.0f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!hasExploded && age >= detonationTime)
        {
            Explode();
        }
    }
    
    protected override void OnCollisionEnter(Collision collision) { }

    public override void Explode()
    {
        if (hasExploded) return;
        if (minePrefab == null)
        {
            Debug.LogError("MineLayerProjectile: Mine Prefab is not assigned!", this);
            base.Explode();
            return;
        }

        hasExploded = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        for (int i = 0; i < mineCount; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle.normalized;
            Vector3 mineVelocity = new Vector3(randomPoint.x, 0, randomPoint.y) * spreadSpeed;

            GameObject mineGO = ObjectPool.Instance.GetFromPool(minePrefab, transform.position, Quaternion.LookRotation(mineVelocity));
            
            Projectile mine = mineGO.GetComponent<Projectile>();
            if (mine != null)
            {
                mine.Initialize(owner, null);
                mine.SetInitialVelocity(mineVelocity);
            }
        }

        ReturnToPool();
    }
}

