using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Setup")]
    public Transform weaponHardpoints;

    private Transform currentTarget;
    private List<WeaponBase> allWeapons = new List<WeaponBase>();

    void Awake()
    {
        if (weaponHardpoints == null)
        {
            PopulateWeaponsFromChildren(transform);
        }
        else
        {
            PopulateWeaponsFromChildren(weaponHardpoints);
        }

        if (allWeapons.Count == 0)
        {
            enabled = false;
        }
    }

    private void PopulateWeaponsFromChildren(Transform parent)
    {
        allWeapons.AddRange(parent.GetComponentsInChildren<WeaponBase>());
    }

    void Update()
    {
        AimWeapons();
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    public List<WeaponBase> GetAllWeapons()
    {
        return allWeapons;
    }

    private void AimWeapons()
    {
        if (currentTarget == null || weaponHardpoints == null) return;

        foreach (var weapon in allWeapons)
        {
            weapon.Aim(currentTarget, transform);
        }
    }

    public void FireAlphaStrike()
    {
        if (allWeapons.Count == 0 || currentTarget == null) return;

        foreach (var weapon in allWeapons)
        {
            // Alpha strike fires all weapons, regardless of fire group.
            FireWeaponIfReady(weapon);
        }
    }
    
    public void FireGroup(int group)
    {
        if (allWeapons.Count == 0 || currentTarget == null) return;

        foreach (var weapon in allWeapons)
        {
            // Fire only if the weapon belongs to the specified group.
            if (weapon.fireGroup == group)
            {
                FireWeaponIfReady(weapon);
            }
        }
    }

    private void FireWeaponIfReady(WeaponBase weapon)
    {
        float distanceToTarget = Vector3.Distance(weapon.transform.position, currentTarget.position);

        // A weapon now only fires if it's within range, can fire, and is physically aimed at the target.
        if (distanceToTarget <= weapon.range && weapon.CanFire() && weapon.IsAimedAt(currentTarget))
        {
            weapon.Fire(currentTarget);
        }
    }
}
