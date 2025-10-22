using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Projectile Properties")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2.0f;
    public int maxAmmo = 0;

    [Header("Missile Launch Options")]
    public float boostDuration = 0.5f;
    public float boostThrust = 50f;
    public Vector3 boostDirection = Vector3.forward;

    private float fireTimer = 0f;
    private int currentAmmo;

    void Awake()
    {
        currentAmmo = maxAmmo;
        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }
    }

    public override bool CanFire()
    {
        bool hasAmmo = (maxAmmo == 0 || currentAmmo > 0);
        return fireTimer <= 0 && hasAmmo;
    }

    public override void Fire(Transform target)
    {
        if (!CanFire() || projectilePrefab == null) return;

        base.Fire(target);

        fireTimer = 1f / fireRate;
        if (maxAmmo > 0) currentAmmo--;

        GameObject projectileGO = ObjectPool.Instance.GetFromPool(projectilePrefab, firePoint.position, firePoint.rotation);
        
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(transform.root, target);

            if (projectile is HomingProjectile homingMissile)
            {
                // Calculate the world-space boost direction here and pass it to the missile.
                Vector3 worldBoostDirection = firePoint.TransformDirection(boostDirection.normalized);
                homingMissile.StartBoostPhase(boostDuration, boostThrust, worldBoostDirection);
            }
        }
    }
}

