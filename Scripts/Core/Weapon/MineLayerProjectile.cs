using UnityEngine;

/// <summary>
/// A homing missile that deploys a cluster of mines at its destination in various patterns.
/// </summary>
public class MineLayerProjectile : HomingProjectile
{
    public enum MinefieldShape { Ring, Line, Grid, V_Formation, Sphere }

    [Header("Mine Layer Properties")]
    public GameObject minePrefab;
    public int mineCount = 8;
    [Tooltip("The deployment pattern for the mines.")]
    public MinefieldShape shape = MinefieldShape.Ring;

    [Header("Shape Parameters")]
    [Tooltip("For Ring/Sphere: The minimum distance from the center to spawn a mine.")]
    public float minRadius = 20f;
    [Tooltip("For Ring/Sphere: The maximum distance from the center to spawn a mine.")]
    public float maxRadius = 50f;
    [Tooltip("For Line/V-Formation: The total length of the line or one arm of the V.")]
    public float length = 100f;
    [Tooltip("For Grid: The distance between each mine in the grid.")]
    public float gridSpacing = 20f;
    [Tooltip("For V-Formation: The angle of the V in degrees.")]
    public float v_Angle = 60f;

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
            ObjectPool.Instance.GetFromPool(explosionPrefab, transform.position, Quaternion.identity);
        }

        DeployMines();

        ReturnToPool();
    }

    private void DeployMines()
    {
        switch (shape)
        {
            case MinefieldShape.Ring:
                DeployRing();
                break;
            case MinefieldShape.Line:
                DeployLine();
                break;
            case MinefieldShape.Grid:
                DeployGrid();
                break;
            case MinefieldShape.V_Formation:
                DeployVFormation();
                break;
            case MinefieldShape.Sphere:
                DeploySphere();
                break;
        }
    }

    private void DeployRing()
    {
        for (int i = 0; i < mineCount; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minRadius, maxRadius);
            Vector3 minePosition = transform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * randomDistance;
            SpawnMine(minePosition, Quaternion.identity, Vector3.zero);
        }
    }

    private void DeployLine()
    {
        Vector3 startPoint = transform.position - transform.forward * (length / 2f);
        Vector3 endPoint = transform.position + transform.forward * (length / 2f);
        for (int i = 0; i < mineCount; i++)
        {
            float t = (float)i / (mineCount - 1);
            Vector3 minePosition = Vector3.Lerp(startPoint, endPoint, t);
            SpawnMine(minePosition, Quaternion.identity, Vector3.zero);
        }
    }

    private void DeployGrid()
    {
        int columns = Mathf.CeilToInt(Mathf.Sqrt(mineCount));
        Vector3 originOffset = new Vector3((columns - 1) * gridSpacing / 2f, 0, (columns - 1) * gridSpacing / 2f);

        for (int i = 0; i < mineCount; i++)
        {
            int row = i / columns;
            int col = i % columns;
            Vector3 minePosition = transform.position + new Vector3(col * gridSpacing, 0, row * gridSpacing) - originOffset;
            SpawnMine(minePosition, Quaternion.identity, Vector3.zero);
        }
    }

    private void DeployVFormation()
    {
        int minesPerArm = Mathf.CeilToInt(mineCount / 2f);
        for (int i = 0; i < mineCount; i++)
        {
            bool isRightArm = i % 2 == 0;
            int armIndex = i / 2;
            float angle = isRightArm ? v_Angle / 2f : -v_Angle / 2f;
            float distance = ((float)armIndex / (minesPerArm - 1)) * length;

            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 minePosition = transform.position + direction * distance;
            SpawnMine(minePosition, Quaternion.identity, Vector3.zero);
        }
    }
    
    private void DeploySphere()
    {
        for (int i = 0; i < mineCount; i++)
        {
            Vector3 minePosition = transform.position + Random.onUnitSphere * Random.Range(minRadius, maxRadius);
            SpawnMine(minePosition, Quaternion.identity, Vector3.zero);
        }
    }

    private void SpawnMine(Vector3 position, Quaternion rotation, Vector3 initialVelocity)
    {
        GameObject mineGO = ObjectPool.Instance.GetFromPool(minePrefab, position, rotation);
        Projectile mine = mineGO.GetComponent<Projectile>();
        if (mine != null)
        {
            mine.Initialize(owner, null);
            mine.SetInitialVelocity(initialVelocity);
        }
    }
}

