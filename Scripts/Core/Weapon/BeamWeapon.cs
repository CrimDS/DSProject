using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamWeapon : WeaponBase
{
    [Header("Beam Properties")]
    public float damagePerSecond = 20f;
    public float duration = 1.0f;
    public float fireRate = 0.5f;

    [Header("Setup")]
    public LineRenderer beamRenderer; 
    public Transform firePoint;
    public ParticleSystem hitEffectPrefab;

    private float fireTimer = 0f;
    private float activeTimer = 0f;
    private ParticleSystem activeHitEffect;
    
    void Awake()
    {
        if (beamRenderer == null)
        {
            beamRenderer = GetComponent<LineRenderer>();
        }
        
        if (beamRenderer == null)
        {
            Debug.LogError("BeamWeapon is missing a LineRenderer reference.", this);
            enabled = false;
            return;
        }

        if (firePoint == null) firePoint = transform;
        beamRenderer.enabled = false;

        if (fireRate <= 0)
        {
            fireRate = 0.001f;
        }
    }

    void Update()
    {
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }

        if (activeTimer > 0)
        {
            activeTimer -= Time.deltaTime;
            UpdateBeam();
        }
        else if (beamRenderer.enabled)
        {
            StopBeam();
        }
    }

    public override bool CanFire()
    {
        return fireTimer <= 0;
    }
    
    public override void Fire(Transform target)
    {
        if (!CanFire()) return;

        base.Fire(target);

        fireTimer = 1f / fireRate;
        activeTimer = duration;
        beamRenderer.enabled = true;

        if (hitEffectPrefab != null && activeHitEffect == null)
        {
            activeHitEffect = Instantiate(hitEffectPrefab, Vector3.zero, Quaternion.identity);
            activeHitEffect.Stop();
        }
    }

    private void UpdateBeam()
    {
        if (firePoint == null) return;

        beamRenderer.SetPosition(0, firePoint.position);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            beamRenderer.SetPosition(1, hit.point);
            
            Damageable damageable = hit.collider.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerSecond * Time.deltaTime);
            }
            
            if (activeHitEffect != null)
            {
                if (!activeHitEffect.isPlaying) activeHitEffect.Play();
                activeHitEffect.transform.position = hit.point;
                activeHitEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
        else
        {
            beamRenderer.SetPosition(1, firePoint.position + firePoint.forward * range);
            if (activeHitEffect != null && activeHitEffect.isPlaying) activeHitEffect.Stop();
        }
    }

    private void StopBeam()
    {
        beamRenderer.enabled = false;
        if (activeHitEffect != null)
        {
            activeHitEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(activeHitEffect.gameObject, activeHitEffect.main.startLifetime.constantMax);
            activeHitEffect = null;
        }
    }
}
