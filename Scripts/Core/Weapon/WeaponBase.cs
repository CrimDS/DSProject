using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A base class for all weapon types, containing shared properties
/// like fire group, firing arc limitations, and range.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Authentic Weapon Properties")]
    [Tooltip("The fire group this weapon belongs to. 0 is for the primary Alpha Strike, 1-9 are custom groups.")]
    [Range(0, 9)]
    public int fireGroup = 0;

    [Tooltip("The designated firing arcs for this weapon.")]
    [EnumFlags]
    public FiringArc firingArc = FiringArc.Fore;

    [Tooltip("The maximum effective range of this weapon in meters.")]
    public float range = 1000f;

    [Tooltip("How fast the weapon can turn to track a target, in degrees per second.")]
    public float turnRate = 90f;

    public event System.Action OnWeaponFired;

    public abstract bool CanFire();
    
    public virtual void Fire(Transform target)
    {
        OnWeaponFired?.Invoke();
    }

    public void Aim(Transform target, Transform shipTransform)
    {
        if (target == null || !IsInArc(shipTransform, target.position))
        {
            return;
        }

        Vector3 directionToTarget = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate * Time.deltaTime);
    }

    public bool IsAimedAt(Transform target)
    {
        if (target == null) return false;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, directionToTarget) > 0.999f;
    }

    public bool IsInArc(Transform shipTransform, Vector3 targetPosition)
    {
        if (firingArc == FiringArc.None) return false;
        if (firingArc == (FiringArc.Fore | FiringArc.Aft | FiringArc.Left | FiringArc.Right)) return true; 

        Vector3 toTarget = (targetPosition - shipTransform.position);
        
        if (toTarget.sqrMagnitude < 25f) return true;

        Vector3 targetDirection = toTarget.normalized;
        float angle = Vector3.SignedAngle(shipTransform.forward, targetDirection, shipTransform.up);

        if ((firingArc & FiringArc.Fore) != 0 && angle > -45 && angle < 45) return true;
        if ((firingArc & FiringArc.Aft) != 0 && (angle > 135 || angle < -135)) return true;
        if ((firingArc & FiringArc.Right) != 0 && angle >= 45 && angle <= 135) return true;
        if ((firingArc & FiringArc.Left) != 0 && angle <= -45 && angle >= -135) return true;

        return false;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Transform shipTransform = transform.root; 
        Handles.color = new Color(1, 0, 0, 0.1f);

        if ((firingArc & FiringArc.Fore) != 0)
            Handles.DrawSolidArc(transform.position, shipTransform.up, Quaternion.Euler(0, -45, 0) * shipTransform.forward, 90, range);
        if ((firingArc & FiringArc.Aft) != 0)
            Handles.DrawSolidArc(transform.position, shipTransform.up, Quaternion.Euler(0, 135, 0) * shipTransform.forward, 90, range);
        if ((firingArc & FiringArc.Right) != 0)
            Handles.DrawSolidArc(transform.position, shipTransform.up, Quaternion.Euler(0, 45, 0) * shipTransform.forward, 90, range);
        if ((firingArc & FiringArc.Left) != 0)
            Handles.DrawSolidArc(transform.position, shipTransform.up, Quaternion.Euler(0, -135, 0) * shipTransform.forward, 90, range);
            
        Handles.color = new Color(1, 1, 0, 0.5f);
        Handles.DrawWireDisc(transform.position, shipTransform.up, range);
    }
#endif
}

[System.Flags]
public enum FiringArc
{
    None = 0,
    Fore = 1 << 0,
    Aft = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
}
